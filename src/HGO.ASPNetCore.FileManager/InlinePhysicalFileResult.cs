using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace HGO.ASPNetCore.FileManager;

/// <summary>
/// Serves a physical file with <c>Content-Disposition: inline</c> and a filename,
/// so browsers that cannot render the content inline still save it with the correct name
/// instead of falling back to the URL's last path segment.
/// </summary>
internal sealed class InlinePhysicalFileResult : IActionResult
{
    private readonly string _physicalPath;
    private readonly string _contentType;
    private readonly string _fileName;
    private readonly bool _enableRangeProcessing;

    public InlinePhysicalFileResult(string physicalPath, string contentType, string fileName, bool enableRangeProcessing = true)
    {
        _physicalPath = physicalPath;
        _contentType = contentType;
        _fileName = fileName;
        _enableRangeProcessing = enableRangeProcessing;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        // Browsers playing video cancel the initial request to issue a range request
        // (seeking, buffering). SendFileAsync throws OperationCanceledException when the
        // client disconnects — this is expected during streaming, not an error.
        try
        {
            var httpContext = context.HttpContext;
            var response = httpContext.Response;
            var fileInfo = new FileInfo(_physicalPath);

            response.ContentType = _contentType;

            // inline disposition WITH filename (the built-in PhysicalFileResult can only do
            // inline-without-name or attachment-with-name; this gives inline-with-name).
            var disposition = new ContentDispositionHeaderValue("inline");
            disposition.SetHttpFileName(_fileName);
            response.Headers.ContentDisposition = disposition.ToString();

            if (_enableRangeProcessing)
            {
                response.Headers.AcceptRanges = "bytes";

                var range = httpContext.Request.GetTypedHeaders().Range;
                if (range is { Ranges.Count: 1 })
                {
                    var rangeItem = range.Ranges.First();
                    if (TryGetRange(rangeItem, fileInfo.Length, out long offset, out long length))
                    {
                        response.StatusCode = StatusCodes.Status206PartialContent;
                        response.Headers.ContentRange = new ContentRangeHeaderValue(offset, offset + length - 1, fileInfo.Length).ToString();
                        response.ContentLength = length;
                        await response.SendFileAsync(_physicalPath, offset, length, httpContext.RequestAborted);
                        return;
                    }

                    // Unsatisfiable range
                    response.StatusCode = StatusCodes.Status416RangeNotSatisfiable;
                    response.Headers.ContentRange = new ContentRangeHeaderValue(fileInfo.Length).ToString();
                    response.ContentLength = 0;
                    return;
                }
            }

            // Full file
            response.ContentLength = fileInfo.Length;
            await response.SendFileAsync(_physicalPath, httpContext.RequestAborted);
        }
        catch (OperationCanceledException) when (context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected during streaming (e.g., browser seeking in a video player).
        }
    }

    private static bool TryGetRange(RangeItemHeaderValue rangeItem, long fileLength, out long offset, out long length)
    {
        offset = 0;
        length = 0;

        if (rangeItem.From.HasValue && rangeItem.To.HasValue)
        {
            offset = rangeItem.From.Value;
            length = rangeItem.To.Value - offset + 1;
        }
        else if (rangeItem.From.HasValue)
        {
            offset = rangeItem.From.Value;
            length = fileLength - offset;
        }
        else if (rangeItem.To.HasValue)
        {
            // Last N bytes
            length = Math.Min(rangeItem.To.Value, fileLength);
            offset = fileLength - length;
        }
        else
        {
            return false;
        }

        return offset >= 0 && offset < fileLength && length > 0 && offset + length <= fileLength;
    }
}
