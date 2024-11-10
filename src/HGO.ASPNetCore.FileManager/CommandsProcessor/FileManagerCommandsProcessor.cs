using HGO.ASPNetCore.FileManager.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using HGO.ASPNetCore.FileManager.ViewComponents;
using SharpCompress.Compressors.Deflate;
using System.Text;
using HGO.ASPNetCore.FileManager.Enums;
using HGO.ASPNetCore.FileManager.ViewModels;
using HGO.ASPNetCore.FileManager.Helpers;
using System.IO;
using System.IO.Pipes;

namespace HGO.ASPNetCore.FileManager.CommandsProcessor;

public class FileManagerCommandsProcessor : IFileManagerCommandsProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly ISession? _session;
    private const string RootPathSessionKey = "HGO-FM-RootFolder";

    public FileManagerCommandsProcessor(IHttpContextAccessor httpContextAccessor, ITempDataProvider tempDataProvider)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempDataProvider = tempDataProvider;
        _session = httpContextAccessor.HttpContext?.Session;
    }

    public async Task<IActionResult> ProcessCommandAsync(string id, Command command, string parameters, IFormFile file)
    {

        if (command == Command.Unknown)
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new { Error = FileManagerComponent.ConfigStorage[id].Language.UnknownCommandErrorMessage })
            };
        }

        var disabledFunctions = FileManagerComponent.ConfigStorage[id].DisabledFunctions;
        if (disabledFunctions.Contains(command))
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new { Error = FileManagerComponent.ConfigStorage[id].Language.ActionDisabledErrorMessage })
            };
        }

        return await Task.Factory.StartNew(() =>
        {
            var result = new ContentResult();
            try
            {
                switch (command)
                {
                    case Command.GetFolderContent:
                        result.Content = GetContent(id, parameters);
                        return result;
                    case Command.Search:
                        result.Content = Search(id, JsonConvert.DeserializeObject<SearchCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.CreateNewFolder:
                        result.Content = CreateNewFolder(id,
                            JsonConvert.DeserializeObject<CreateNewFolderCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.CreateNewFile:
                        result.Content = CreateNewFile(id,
                            JsonConvert.DeserializeObject<CreateNewFileCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.Delete:
                        result.Content = Delete(id,
                            JsonConvert.DeserializeObject<DeleteCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.Rename:
                        result.Content = Rename(id,
                            JsonConvert.DeserializeObject<RenameCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.Zip:
                        result.Content = Zip(id,
                            JsonConvert.DeserializeObject<ZipCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.Unzip:
                        result.Content = UnZip(id,
                            JsonConvert.DeserializeObject<UnZipCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.Copy:
                    case Command.Cut:
                        result.Content = CopyCutItems(id, command,
                            JsonConvert.DeserializeObject<CopyCutCommandParameters>(parameters) ?? new());
                        return result;
                    case Command.EditFile:
                        return EditFile(id,
                            JsonConvert.DeserializeObject<EditFileCommandParameters>(parameters) ?? new());
                    case Command.Download:
                        return Download(id, parameters, false);
                    case Command.View:
                        return Download(id, parameters, true);
                    case Command.GetFileContent:
                        return GetFileContent(id, parameters);
                    case Command.Upload:
                        return Upload(id, parameters, file);
                    case Command.FilePreview:
                        return FilePreview(id, parameters);
                    default:
                        result.Content = JsonConvert.SerializeObject(new { Error = FileManagerComponent.ConfigStorage[id].Language.UnknownCommandErrorMessage });
                        return result;
                }
            }
            catch (Exception e)
            {
                var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
                result.Content = JsonConvert.SerializeObject(string.IsNullOrWhiteSpace(physicalRootPath)
                    ? new { Error = e.Message }
                    : new { Error = e.Message.Replace(physicalRootPath.TrimEnd(Path.DirectorySeparatorChar), "Root") });
                return result;
            }
        });
    }

    private string? GetCurrentSessionPhysicalRootPath(string id)
    {
        var sessionKey = RootPathSessionKey + id;
        if (_session == null || string.IsNullOrWhiteSpace(_session.GetString(sessionKey)))
        {
            return null;
        }

        if (!Directory.Exists(_session.GetString(sessionKey)))
        {
            return null;
        }

        return Path.GetFullPath(_session.GetString(sessionKey)?.Trim() ?? "");
    }

    private long GetRootFolderSize(string id)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (Directory.Exists(physicalRootPath))
        {
            return new DirectoryInfo(physicalRootPath).GetFiles("*", SearchOption.AllDirectories).Sum(p => p.Length) / 1024 / 1024;
        }
        return 0;
    }

    private string GetContent(string id, string virtualPath)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = virtualPath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);
        var result = new GetContentResultModel()
        {
            CurrentPath = physicalPath.ConvertPhysicalToVirtualPath(physicalRootPath),

            Files = Utils.GetFiles(physicalPath, "*.*", SearchOption.TopDirectoryOnly)
            .Select(p => p.GetFileDetail(physicalRootPath, encryptionHelper)).ToList(),

            Folders = Utils.GetDirectories(physicalPath, "*.*", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)).ToList(),
        };


        return result.ToString();
    }

    private string Search(string id, SearchCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        var searchPattern = commandParameters.Query;
        if (string.IsNullOrWhiteSpace(commandParameters.Query))
        {
            searchPattern = "*";
        }

        var result = new GetContentResultModel()
        {
            CurrentPath = physicalPath.ConvertPhysicalToVirtualPath(physicalRootPath),
        };
        var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);
        if (searchPattern.Contains("*"))
        {
            result.Files.AddRange(Utils.GetFiles(physicalPath, searchPattern, SearchOption.AllDirectories).OrderBy(p => p)
                .Select(p => p.GetFileDetail(physicalRootPath, encryptionHelper)));

            result.Folders.AddRange(Utils.GetDirectories(physicalPath, searchPattern, SearchOption.AllDirectories)
                .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)));
        }
        else
        {
            result.Files.AddRange(Utils.GetFiles(physicalPath, "*", SearchOption.AllDirectories).OrderBy(p => p)
                .Select(p => p.GetFileDetail(physicalRootPath, encryptionHelper)));
            result.Folders.AddRange(Utils.GetDirectories(physicalPath, "*", SearchOption.AllDirectories)
                .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)));

            result.Files.RemoveAll(p =>
                !p!.FileName.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase));
            result.Folders.RemoveAll(p =>
                !p!.FolderName.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase));
        }

        return result.ToString();
    }

    private string CreateNewFolder(string id, CreateNewFolderCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //create new folder
        var newFolder = Path.Combine(physicalPath, commandParameters.FolderName);
        var tmp = newFolder;
        var counter = 1;
        while (Directory.Exists(tmp))
        {
            tmp = newFolder + $" ({counter})";
            counter++;
        }
        newFolder = tmp;
        Directory.CreateDirectory(newFolder);

        return GetContent(id, commandParameters.Path);
    }

    private string CreateNewFile(string id, CreateNewFileCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //create new file
        var newFile = Path.Combine(physicalPath, commandParameters.FileName);
        var tmp = newFile;
        var counter = 1;
        while (File.Exists(tmp))
        {
            var ext = Path.GetExtension(newFile);
            tmp = newFile.TrimEnd(ext) + $" ({counter})" + ext;
            counter++;
        }
        newFile = tmp;
        File.WriteAllText(newFile, "");

        return GetContent(id, commandParameters.Path);
    }

    private string Delete(string id, DeleteCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //Delete selected files/folders
        foreach (var item in commandParameters.Items)
        {
            var physicalItemPathToDelete = item.ConvertVirtualToPhysicalPath(physicalRootPath);

            if (physicalItemPathToDelete.ToLower().StartsWith(physicalPath.ToLower()))
            {
                if (Directory.Exists(physicalItemPathToDelete))
                {
                    Directory.Delete(physicalItemPathToDelete, true);
                }
                else if (File.Exists(physicalItemPathToDelete))
                {
                    File.Delete(physicalItemPathToDelete);
                }
            }
        }

        return GetContent(id, commandParameters.Path);
    }

    private string Rename(string id, RenameCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //Rename files/folders
        foreach (var item in commandParameters.Items)
        {
            var physicalItemPathToRename = item.ConvertVirtualToPhysicalPath(physicalRootPath);
            int dirCounter = 1;
            int fileCounter = 1;

            if (Directory.Exists(physicalItemPathToRename))
            {
                var folderNewName = Path.Combine(physicalPath, commandParameters.NewName);

                if (folderNewName.ToLower().Trim() == physicalItemPathToRename.ToLower().Trim())
                {
                    continue;
                }

                while (Directory.Exists(folderNewName))
                {
                    folderNewName = Path.Combine(physicalPath, commandParameters.NewName + $" ({dirCounter})");
                    dirCounter++;
                }

                Directory.Move(physicalItemPathToRename, folderNewName);
            }

            if (File.Exists(physicalItemPathToRename))
            {
                var ext = Path.GetExtension(commandParameters.NewName);
                if (string.IsNullOrWhiteSpace(ext))
                    ext = Path.GetExtension(physicalItemPathToRename);

                var newName = commandParameters.NewName.TrimEnd(ext);

                var fileNewName = Path.Combine(physicalPath, newName + ext);

                if (fileNewName.ToLower().Trim() == physicalItemPathToRename.ToLower().Trim())
                {
                    continue;
                }

                while (File.Exists(fileNewName))
                {
                    fileNewName = Path.Combine(physicalPath, newName + $" ({fileCounter})" + ext);
                    fileCounter++;
                }

                File.Move(physicalItemPathToRename, fileNewName, true);
            }
        }

        return GetContent(id, commandParameters.Path);
    }

    private string Zip(string id, ZipCommandParameters commandParameters)
    {
        var storageSizeLimit = FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte;
        var rootFolderSize = GetRootFolderSize(id);
        if (storageSizeLimit > 0 && storageSizeLimit < rootFolderSize)
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage);
        }

        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);
        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        var newZipFileName = Path.Combine(physicalPath, commandParameters.FileName.TrimStart(Path.DirectorySeparatorChar).TrimEnd(".zip"));
        var tmp = newZipFileName;
        int counter = 1;
        while (File.Exists(newZipFileName + ".zip"))
        {
            newZipFileName = tmp + $" ({counter})";
            counter++;
        }
        newZipFileName += ".zip";

        var compressionMaxSize = FileManagerComponent.ConfigStorage[id].CompressionMaxSizeMByte;
        if (compressionMaxSize > 0 || storageSizeLimit > 0)
        {
            long allFilesSize = 0;
            foreach (var item in commandParameters.Items.Select(p => p.ConvertVirtualToPhysicalPath(physicalRootPath)))
            {
                if (Directory.Exists(item))
                {
                    allFilesSize += new DirectoryInfo(item).GetFiles("*", SearchOption.AllDirectories).Sum(p => p.Length);
                }

                if (File.Exists(item))
                {
                    allFilesSize += new FileInfo(item).Length;
                }
            }

            allFilesSize = allFilesSize / 1024 / 1024;
            if (compressionMaxSize > 0 && compressionMaxSize < allFilesSize)
            {
                throw new Exception(FileManagerComponent.ConfigStorage[id].Language.TooBigErrorMessage);
            }

            if (storageSizeLimit > 0 && storageSizeLimit < rootFolderSize + allFilesSize)
            {
                throw new Exception(FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage);
            }
        }

        // Create a temporary memory stream for the unencrypted zip archive
        using (var zipMemoryStream = new MemoryStream())
        using (var archive = ZipArchive.Create())
        {
            archive.DeflateCompressionLevel = (CompressionLevel)FileManagerComponent.ConfigStorage[id].CompressionLevel;


            var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);

            foreach (string item in commandParameters.Items.Select(p => p.ConvertVirtualToPhysicalPath(physicalRootPath)))
            {
                if (Directory.Exists(item))
                {
                    foreach (string file in Utils.GetFiles(item, "*.*", SearchOption.AllDirectories))
                    {
                        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {
                            // Decrypt file if necessary before adding to zip
                            var decryptedStream = encryptionHelper.DecryptStream(fileStream);
                            archive.AddEntry(file.TrimStart(physicalPath), decryptedStream);
                        }
                    }
                }

                if (File.Exists(item))
                {
                    using (var fileStream = new FileStream(item, FileMode.Open, FileAccess.Read))
                    {
                        var decryptedStream = encryptionHelper.DecryptStream(fileStream);
                        archive.AddEntry(Path.GetFileName(item), decryptedStream);
                    }
                }
            }

            archive.SaveTo(zipMemoryStream, new WriterOptions(CompressionType.Deflate) { LeaveStreamOpen = true });
            zipMemoryStream.Position = 0; // Reset stream position for reading

            // Encrypt the zip archive stream
            using (var encryptedZipStream = encryptionHelper.EncryptStream(zipMemoryStream))
            {
                encryptionHelper.SaveStreamToFile(encryptedZipStream, newZipFileName); // Save encrypted zip to file
            }
        }


        return GetContent(id, commandParameters.Path);
    }

    private string UnZip(string id, UnZipCommandParameters commandParameters)
    {
        var storageSizeLimit = FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte;
        var rootFolderSize = GetRootFolderSize(id);

        if (storageSizeLimit > 0 && storageSizeLimit < rootFolderSize)
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage);
        }

        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        // Compute all selected zip files size
        if (storageSizeLimit > 0)
        {
            long allFilesSize = commandParameters.Items.Select(p => p.ConvertVirtualToPhysicalPath(physicalRootPath))
                .Where(item => File.Exists(item)).Sum(item => new FileInfo(item).Length);

            allFilesSize = allFilesSize / 1024 / 1024;

            if (storageSizeLimit < rootFolderSize + allFilesSize)
            {
                throw new Exception(FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage);
            }
        }

        // Decompress each zip archive and re-encrypt each extracted file
        var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);

        foreach (var item in commandParameters.Items)
        {
            var zipArchivePhysicalPath = item.ConvertVirtualToPhysicalPath(physicalRootPath);

            if (File.Exists(zipArchivePhysicalPath))
            {
                using var encryptedStream = File.OpenRead(zipArchivePhysicalPath);
                using var decryptedStream = encryptionHelper.DecryptStream(encryptedStream);
                using var reader = ReaderFactory.Open(decryptedStream);

                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        var entryPath = Path.Combine(physicalPath, reader.Entry.Key);
                        Directory.CreateDirectory(Path.GetDirectoryName(entryPath));

                        using var extractedStream = new MemoryStream();
                        reader.WriteEntryTo(extractedStream); // Extract to memory stream
                        extractedStream.Position = 0; // Reset stream position for reading

                        // Encrypt the extracted content before saving to disk
                        using var encryptedExtractedStream = encryptionHelper.EncryptStream(extractedStream);
                        encryptionHelper.SaveStreamToFile(encryptedExtractedStream, entryPath);
                    }
                }
            }
        }

        return GetContent(id, commandParameters.Path);
    }

    private string CopyCutItems(string id, Command action, CopyCutCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception(FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage);
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //Copy/Move Files/Folders
        foreach (var item in commandParameters.Items)
        {
            var physicalItemPathToCopy = item.ConvertVirtualToPhysicalPath(physicalRootPath);

            if (File.Exists(physicalItemPathToCopy))
            {
                if (action == Command.Copy)
                {
                    File.Copy(physicalItemPathToCopy, Path.Combine(physicalPath, Path.GetFileName(physicalItemPathToCopy)), true);
                }
                else if (action == Command.Cut)
                {
                    File.Move(physicalItemPathToCopy, Path.Combine(physicalPath, Path.GetFileName(physicalItemPathToCopy)), true);
                }
            }
            else if (Directory.Exists(physicalItemPathToCopy))
            {
                Utils.CopyDirectory(physicalItemPathToCopy, physicalPath, true);
                if (action == Command.Cut)
                {
                    Directory.Delete(physicalItemPathToCopy, true);
                }
            }
        }

        return GetContent(id, commandParameters.Path);
    }

    private IActionResult Download(string id, string filePath, bool view)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage,
                StatusCode = 500 // Internal Server Error
            };
        }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        // Validate path security and existence
        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage,
                StatusCode = 404 // Not Found
            };
        }

        // Decrypt file if encryption is enabled
        var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);
        Stream fileStream = encryptionHelper.DecryptStream(File.OpenRead(physicalPath)); // Decrypt directly into stream
        
        var mimeType = Utils.GetMimeTypeForFileExtension(physicalPath);
        var fileStreamResult = new FileStreamResult(fileStream, mimeType);

        // Set Content-Type header explicitly
        _httpContextAccessor!.HttpContext!.Response.ContentType = mimeType;

        // Handle inline viewing for text files
        if (view && mimeType == "text/plain")
        {
            // Read the decrypted file into a byte array
            byte[] decryptedBytes;
            using (var ms = new MemoryStream())
            {
                fileStream.CopyTo(ms);
                decryptedBytes = ms.ToArray();
            }

            // Remove or replace null bytes (0x00) from the byte array
            decryptedBytes = RemoveNullBytes(decryptedBytes);

            // Convert the cleaned byte array back to a UTF-8 string
            string fileContent = Encoding.UTF8.GetString(decryptedBytes);

            // Convert the content back into a stream and send it to the browser
            var contentStream = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            fileStreamResult = new FileStreamResult(contentStream, "text/plain");
            fileStreamResult.EnableRangeProcessing = false; // Disable range processing for text files

            // Force inline display for text files
            _httpContextAccessor.HttpContext.Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Path.GetFileName(physicalPath)}\"");
            _httpContextAccessor.HttpContext.Response.Headers.Append("Content-Type", "text/plain; charset=utf-8");
        }
        else if (view && (mimeType.StartsWith("image/") || mimeType == "application/pdf"))
        {
            // Images and PDFs should be displayed inline
            _httpContextAccessor.HttpContext.Response.Headers.Append("Content-Disposition", $"inline; filename=\"{Path.GetFileName(physicalPath)}\"");
        }
        else
        {
            // If not inline, force download
            _httpContextAccessor.HttpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{Path.GetFileName(physicalPath)}\"");
        }

        // Optional headers to ensure inline viewing is respected
        _httpContextAccessor.HttpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        _httpContextAccessor.HttpContext.Response.Headers.Append("Cache-Control", "no-cache");

        return fileStreamResult;
    }

    // Helper method to remove or replace null bytes from the decrypted byte array
    private byte[] RemoveNullBytes(byte[] bytes)
    {
        // Filter out null bytes (0x00) from the byte array
        return bytes.Where(b => b != 0x00).ToArray();
    }




    private IActionResult GetFileContent(string id, string filePath)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage,
                StatusCode = 500 // Internal Server Error
            };
        }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        // Validate path security and existence
        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage,
                StatusCode = 404 // Not Found
            };
        }

        // Check if the file is binary; if so, it’s not editable
        if (Utils.IsBinary(physicalPath))
        {
            return new ContentResult
            {
                Content = Path.GetFileName(physicalPath) + FileManagerComponent.ConfigStorage[id].Language.IsNotEditableFileErrorMessage,
                StatusCode = 400 // Bad Request
            };
        }

        string fileData = string.Empty;
        try
        {
            var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);

            using (var decryptedStream = encryptionHelper.DecryptStream(File.OpenRead(physicalPath)))
            using (var reader = new StreamReader(decryptedStream, Encoding.UTF8)) // Use UTF-8 encoding to read the content
            {
                // Read the decrypted file into a byte array
                byte[] decryptedBytes;
                using (var ms = new MemoryStream())
                {
                    decryptedStream.CopyTo(ms);
                    decryptedBytes = ms.ToArray();
                }

                // Remove or replace null bytes (0x00) from the byte array
                decryptedBytes = RemoveNullBytes(decryptedBytes);

                // Convert the cleaned byte array back to a UTF-8 string
                fileData = Encoding.UTF8.GetString(decryptedBytes);
            }
        }
        catch (Exception ex)
        {
            // Handle errors during decryption or reading
            return new ContentResult
            {
                Content = $"Error reading file: {ex.Message}",
                StatusCode = 500 // Internal Server Error
            };
        }

        // Create and populate the view model
        var viewModel = new EditViewModel
        {
            Id = id,
            FileFullPath = filePath,
            FileName = Path.GetFileName(physicalPath),
            FileData = fileData,
            Language = FileManagerComponent.ConfigStorage[id].Language,
        };

        // Prepare the view result with the view model in TempData
        var result = new ViewResult
        {
            ViewName = "HgoFileManager/Edit",
            TempData = new TempDataDictionary(_httpContextAccessor.HttpContext!, _tempDataProvider),
        };
        result.TempData["model"] = viewModel;

        return result;
    }


    private IActionResult EditFile(string id, EditFileCommandParameters commandParameters)
    {
        var storageSizeLimit = FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte;
        var fileContentSize = Encoding.Unicode.GetByteCount(commandParameters.Data) / 1024 / 1024;
        if (storageSizeLimit > 0 && storageSizeLimit < GetRootFolderSize(id) + fileContentSize)
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage,
                })
            };
        }

        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage
                })
            };
        }

        var physicalPath = commandParameters.FilePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage
                })
            };
        }

        try
        {
            // Initialize encryption helper with the encryption key and usage flag from configuration
            var encryptionHelper = new FileEncryptionHelper(
                FileManagerComponent.ConfigStorage[id].EncryptionKey,
                FileManagerComponent.ConfigStorage[id].UseEncryption
            );

            using (var originalFileStream = new FileStream(physicalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                // Check if the file needs to be decrypted
                if (encryptionHelper.IsEncrypted(originalFileStream) && !FileManagerComponent.ConfigStorage[id].UseEncryption)
                {
                    return new ContentResult
                    {
                        Content = JsonConvert.SerializeObject(new
                        {
                            message = " The file is encrypted and cannot be edited without decrypting it first."
                        })
                    };
                }


                // Reset the file stream for writing new content
                originalFileStream.SetLength(0); // Clears the file to avoid overwriting issues
                originalFileStream.Position = 0; // Reset position for overwriting

                // Convert data to a stream and encrypt if encryption is enabled
                using var encryptedStream = encryptionHelper.EncryptStream(new MemoryStream(Encoding.Unicode.GetBytes(commandParameters.Data)));

                // Copy the encrypted content into the file stream
                encryptedStream.CopyTo(originalFileStream);

            }

            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new { message = "OK" })
            };
        }
        catch (Exception e)
        {
            return new ContentResult
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = e.Message.Replace(physicalRootPath.TrimEnd(Path.DirectorySeparatorChar), "Root")
                })
            };
        }
    }

    private bool AreStreamsEqual(Stream stream1, Stream stream2)
    {
        if (stream1 == null || stream2 == null) return false;

        // Ensure both streams are at the start
        stream1.Position = 0;
        stream2.Position = 0;

        // Quick length check
        if (stream1.Length != stream2.Length) return false;

        // Compare content byte by byte
        int byte1, byte2;
        while ((byte1 = stream1.ReadByte()) != -1 &&
               (byte2 = stream2.ReadByte()) != -1)
        {
            if (byte1 != byte2) return false;
        }

        return true;
    }

    public IActionResult Upload(string id, string path, IFormFile file)
    {
        // Cache configuration
        var fileSizeInMB = file.Length / 1024.0 / 1024.0;

        // Validate storage size limit
        if (FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte > 0 && FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte < GetRootFolderSize(id) + fileSizeInMB)
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage,
                StatusCode = 400
            };
        }

        // Validate file size limit
        if (FileManagerComponent.ConfigStorage[id].MaxFileSizeToUploadMByte > 0 && FileManagerComponent.ConfigStorage[id].MaxFileSizeToUploadMByte < fileSizeInMB)
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.TooBigErrorMessage,
                StatusCode = 400
            };
        }

        // Validate file extension
        if (!string.IsNullOrWhiteSpace(FileManagerComponent.ConfigStorage[id].AcceptedFiles))
        {
            var fileExt = Path.GetExtension(file.FileName).ToLower().Trim();
            if (!FileManagerComponent.ConfigStorage[id].AcceptedFiles.ToLower().Contains(fileExt))
            {
                return new ContentResult
                {
                    Content = $"'{fileExt}' {FileManagerComponent.ConfigStorage[id].Language.FilesNotAcceptedErrorMessage}",
                    StatusCode = 400
                };
            }
        }

        // Verify and prepare paths
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage,
                StatusCode = 400
            };
        }

        var physicalPath = path.ConvertVirtualToPhysicalPath(physicalRootPath);
        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        var filePath = Path.Combine(physicalPath, file.FileName);

        // Handle chunked uploads for existing files
        if (File.Exists(filePath) && _httpContextAccessor.HttpContext?.Request.Query.TryGetValue("chunkIndex", out var chunkIndex) == true)
        {
            if (int.TryParse(chunkIndex.ToString(), out int idx) && idx == 0)
            {
                return new ContentResult
                {
                    Content = FileManagerComponent.ConfigStorage[id].Language.FileAlreadyExistsErrorMessage,
                    StatusCode = 400
                };
            }
        }

        // Encrypt and save the file
        using (var fileStream = new FileStream(filePath, FileMode.Append))
        {
            var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);
            using (var encryptedStream = encryptionHelper.EncryptStream(file.OpenReadStream()))
            {
                encryptedStream.CopyTo(fileStream);
            }
        }

        return new OkResult();
    }



    private IActionResult FilePreview(string id, string filePath)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult()
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.InvalidRootPathErrorMessage,
                StatusCode = 500//server error
            };
        }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult()
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage,
                StatusCode = 404 //not found
            };
        }

        switch (Path.GetExtension(physicalPath).ToLower().Trim())
        {
            case ".png" or ".jpg" or ".webp" or ".gif" or ".svg" or ".jpeg" or ".apng" or ".avif" or ".ico" or ".bmp" or ".tif" or ".tiff":
                // Decrypt file if encryption is enabled
                var encryptionHelper = new FileEncryptionHelper(FileManagerComponent.ConfigStorage[id].EncryptionKey, FileManagerComponent.ConfigStorage[id].UseEncryption);
                //using (Stream fileStream = encryptionHelper.DecryptStream(File.OpenRead(physicalPath)))
                //{
                Stream fileStream = encryptionHelper.DecryptStream(File.OpenRead(physicalPath)); // Decrypt directly into stream

                var mimeType = Utils.GetMimeTypeForFileExtension(physicalPath);
                var fileStreamResult = new FileStreamResult(fileStream, mimeType)
                {
                    FileDownloadName = Path.GetFileName(physicalPath),
                    EnableRangeProcessing = true
                };
                return fileStreamResult;
            //}
            case ".zip" or ".rar" or ".tar" or ".7z" or ".gzip" or ".7zip":
                return new RedirectResult("/hgofilemanager/images/zip.png") { Permanent = true };
            case ".js" or ".jsx":
                return new RedirectResult("/hgofilemanager/images/js.png") { Permanent = true };
            case ".php":
                return new RedirectResult("/hgofilemanager/images/php.png") { Permanent = true };
            case ".html" or ".htm" or ".cshtml":
                return new RedirectResult("/hgofilemanager/images/html.png") { Permanent = true };
            case ".css":
                return new RedirectResult("/hgofilemanager/images/css.png") { Permanent = true };
            case ".json":
                return new RedirectResult("/hgofilemanager/images/json.png") { Permanent = true };
            case ".txt":
                return new RedirectResult("/hgofilemanager/images/txt.png") { Permanent = true };
            case ".pdf":
                return new RedirectResult("/hgofilemanager/images/pdf.png") { Permanent = true };
            case ".mp4":
                return new RedirectResult("/hgofilemanager/images/mp4.png") { Permanent = true };
            case ".mp3":
                return new RedirectResult("/hgofilemanager/images/mp3.png") { Permanent = true };
            case ".exe":
                return new RedirectResult("/hgofilemanager/images/exe.png") { Permanent = true };
            case ".dll":
                return new RedirectResult("/hgofilemanager/images/dll.png") { Permanent = true };
            default:
                return new RedirectResult("/hgofilemanager/images/file.png") { Permanent = true };
        }
    }
}