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
using System.IO;
using SharpCompress.Compressors.Deflate;

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
        _session = httpContextAccessor.HttpContext.Session;
    }

    public async Task<IActionResult> ProcessCommandAsync(string id, string command, string parameters, IFormFile file)
    {

        if (string.IsNullOrWhiteSpace(command))
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new { Error = "Unknown command!" })
            };
        }

        return await Task.Factory.StartNew(() =>
        {
            var result = new ContentResult();
            try
            {
                switch (command.ToLower().Trim())
                {
                    case "getfoldercontent":
                        result.Content = GetContent(id, parameters);
                        return result;
                    case "search":
                        result.Content = Search(id, JsonConvert.DeserializeObject<SearchCommandParameters>(parameters));
                        return result;
                    case "createnewfolder":
                        result.Content = CreateNewFolder(id,
                            JsonConvert.DeserializeObject<CreateNewFolderCommandParameters>(parameters));
                        return result;
                    case "createnewfile":
                        result.Content = CreateNewFile(id,
                            JsonConvert.DeserializeObject<CreateNewFileCommandParameters>(parameters));
                        return result;
                    case "deleteitems":
                        result.Content = DeleteItems(id,
                            JsonConvert.DeserializeObject<DeleteItemsCommandParameters>(parameters));
                        return result;
                    case "renameitems":
                        result.Content = RenameItems(id,
                            JsonConvert.DeserializeObject<RenameItemsCommandParameters>(parameters));
                        return result;
                    case "zipitems":
                        result.Content = ZipItems(id,
                            JsonConvert.DeserializeObject<ZipItemsCommandParameters>(parameters));
                        return result;
                    case "extractitems":
                        result.Content = ExtractItems(id,
                            JsonConvert.DeserializeObject<ExtractItemsCommandParameters>(parameters));
                        return result;
                    case "copy":
                    case "cut":
                        result.Content = CopyCutItems(id, command.ToLower(),
                            JsonConvert.DeserializeObject<CopyCutCommandParameters>(parameters));
                        return result;
                    case "editfilecontent":
                        return EditFileContent(id,
                            JsonConvert.DeserializeObject<EditFileCommandParameters>(parameters));
                    case "downloaditem":
                        return DownloadItem(id, parameters);
                    case "showfilecontent":
                        return ShowFileContent(id, parameters);
                    case "upload":
                        return Upload(id, parameters, file);
                    case "filepreview":
                        return FilePreview(id, parameters);
                    default:
                        result.Content = JsonConvert.SerializeObject(new { Error = "Unknown command!" });
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

    public string? GetCurrentSessionPhysicalRootPath(string id)
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

        return Path.GetFullPath(_session.GetString(sessionKey).Trim());
    }

    private string GetContent(string id, string virtualPath)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
        }

        var physicalPath = virtualPath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        var result = new GetContentResultModel()
        {
            CurrentPath = physicalPath.ConvertPhysicalToVirtualPath(physicalRootPath),
        };

        result.Files.AddRange(Utils.GetFiles(physicalPath, "*.*", SearchOption.TopDirectoryOnly)
            .Select(p => p.GetFileDetail(physicalRootPath)));
        result.Folders.AddRange(Utils.GetDirectories(physicalPath, "*.*", SearchOption.TopDirectoryOnly)
            .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)));

        return result.ToString();
    }

    private string Search(string id, SearchCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        var searchPattern = commandParameters.Query;
        if (string.IsNullOrWhiteSpace(commandParameters.Query))
        {
            searchPattern = "*.*";
        }

        var result = new GetContentResultModel()
        {
            CurrentPath = physicalPath.ConvertPhysicalToVirtualPath(physicalRootPath),
        };

        result.Files.AddRange(Utils.GetFiles(physicalPath, searchPattern, SearchOption.AllDirectories).OrderBy(p => p)
            .Select(p => p.GetFileDetail(physicalRootPath)));
        result.Folders.AddRange(Utils.GetDirectories(physicalPath, searchPattern, SearchOption.AllDirectories)
            .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)));

        return result.ToString();
    }

    private string CreateNewFolder(string id, CreateNewFolderCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
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
            throw new Exception("Invalid Root Path!");
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

    private string DeleteItems(string id, DeleteItemsCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
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

    private string RenameItems(string id, RenameItemsCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
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

    private string ZipItems(string id, ZipItemsCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //check if file exist
        var newZipFileName = Path.Combine(physicalPath, commandParameters.FileName.TrimStart(Path.DirectorySeparatorChar).TrimEnd(".zip"));
        var tmp = newZipFileName;
        int counter = 1;
        while (File.Exists(newZipFileName + ".zip"))
        {
            newZipFileName = tmp + $" ({counter})";
            counter++;
        }
        newZipFileName += ".zip";

        //create Zip archive
        using (Stream stream = File.Create(newZipFileName))
        using (var archive = ZipArchive.Create())
        {
            archive.DeflateCompressionLevel = CompressionLevel.Default;

            foreach (var item in commandParameters.Items.Select(p => p.ConvertVirtualToPhysicalPath(physicalRootPath)))
            {
                if (Directory.Exists(item))
                {
                    foreach (var file in Utils.GetFiles(item, "*.*", SearchOption.AllDirectories))
                    {
                        archive.AddEntry(file.TrimStart(physicalPath), file);
                    }
                }

                if (File.Exists(item))
                {
                    archive.AddEntry(Path.GetFileName(item), item);
                }
            }

            archive.SaveTo(stream, new WriterOptions(CompressionType.Deflate)
            {
                LeaveStreamOpen = false
            });
        }

        return GetContent(id, commandParameters.Path);
    }

    private string ExtractItems(string id, ExtractItemsCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
        }

        var physicalPath = commandParameters.Path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        //decompress zip archive
        foreach (var item in commandParameters.Items)
        {
            var zipArchivePhysicalPath = item.ConvertVirtualToPhysicalPath(physicalRootPath);

            if (File.Exists(zipArchivePhysicalPath))
            {
                using (Stream stream = File.OpenRead(zipArchivePhysicalPath))
                using (var reader = ReaderFactory.Open(stream))
                {
                    reader.WriteAllToDirectory(physicalPath, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true,
                    });
                }
            }
        }

        return GetContent(id, commandParameters.Path);
    }

    private string CopyCutItems(string id, string action, CopyCutCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
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
                if (action.Trim().ToLower() == "copy")
                {
                    File.Copy(physicalItemPathToCopy, Path.Combine(physicalPath, Path.GetFileName(physicalItemPathToCopy)), true);
                }
                else if (action.Trim().ToLower() == "cut")
                {
                    File.Move(physicalItemPathToCopy, Path.Combine(physicalPath, Path.GetFileName(physicalItemPathToCopy)), true);
                }
            }
            else if (Directory.Exists(physicalItemPathToCopy))
            {
                Utils.CopyDirectory(physicalItemPathToCopy, physicalPath, true);
                if (action.Trim().ToLower() == "cut")
                {
                    Directory.Delete(physicalItemPathToCopy, true);
                }
            }
        }

        return GetContent(id, commandParameters.Path);
    }

    private IActionResult DownloadItem(string id, string filePath)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult()
            {
                Content = "Invalid Root Path!",
                StatusCode = 500//server error
            };
        }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult()
            {
                Content = "Your requested resource was not found!",
                StatusCode = 404 //not found
            };
        }

        return new PhysicalFileResult(physicalPath, Utils.GetMimeTypeForFileExtension(physicalPath))
        {
            FileDownloadName = Path.GetFileName(physicalPath),
            EnableRangeProcessing = true
        };
    }

    private IActionResult ShowFileContent(string id, string filePath)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult()
            {
                Content = "Invalid Root Path!",
                StatusCode = 500//server error
            };
        }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult()
            {
                Content = "Your requested resource was not found!",
                StatusCode = 404 //not found
            };
        }

        if (Utils.IsBinary(physicalPath))
        {
            return new ContentResult()
            {
                Content = Path.GetFileName(physicalPath) + " is not editable file!",
                StatusCode = 400 //bad Request
            };
        }

        var result = new ViewResult()
        {
            ViewName = "HgoFileManager/Edit",
            TempData = new TempDataDictionary(_httpContextAccessor.HttpContext, _tempDataProvider)
        };
        result.TempData["Id"] = id;
        result.TempData["FileFullPath"] = filePath;
        result.TempData["FileName"] = Path.GetFileName(physicalPath);
        result.TempData["FileData"] = File.ReadAllText(physicalPath);

        return result;
    }

    private IActionResult EditFileContent(string id, EditFileCommandParameters commandParameters)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = "Invalid Root Path!"
                })
            };
        }

        var physicalPath = commandParameters.FilePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = "Your requested resource was not found!"
                })
            };
        }

        try
        {
            File.WriteAllText(physicalPath, commandParameters.Data);
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new { message = "OK" })
            };
        }
        catch (Exception e)
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = e.Message.Replace(physicalRootPath.TrimEnd(Path.DirectorySeparatorChar), "Root")
                })
            };
        }
    }

    private IActionResult Upload(string id, string path, IFormFile file)
    {
        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            throw new Exception("Invalid Root Path!");
        }

        var physicalPath = path.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = physicalRootPath;
        }

        if (file.Length > 0)
        {
            var filePath = Path.Combine(physicalPath, file.FileName);

            if (File.Exists(filePath) && _httpContextAccessor.HttpContext.Request.Form.TryGetValue("dzchunkindex", out StringValues chunkIndex))
            {
                if (int.TryParse(chunkIndex.ToString(), out int idx) && idx == 0)
                {
                    throw new Exception("File already exist");
                }
            }

            using Stream fileStream = new FileStream(filePath, FileMode.Append);
            file.CopyTo(fileStream);
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
                Content = "Invalid Root Path!",
                StatusCode = 500//server error
            };
        }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(physicalRootPath);

        if (!physicalPath.ToLower().StartsWith(physicalRootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new ContentResult()
            {
                Content = "Your requested resource was not found!",
                StatusCode = 404 //not found
            };
        }

        switch (Path.GetExtension(physicalPath).ToLower().Trim())
        {
            case ".png" or ".jpg" or ".webp" or ".gif" or ".svg" or ".jpeg" or ".apng" or ".avif" or ".ico" or ".bmp" or ".tif" or ".tiff":
                return new PhysicalFileResult(physicalPath, Utils.GetMimeTypeForFileExtension(physicalPath))
                {
                    FileDownloadName = Path.GetFileName(physicalPath),
                    EnableRangeProcessing = true
                };
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
            default:
                return new RedirectResult("/hgofilemanager/images/file.png") { Permanent = true };
        }
    }
}