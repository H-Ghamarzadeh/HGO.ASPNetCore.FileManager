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
                        result.Content = Search(id, JsonConvert.DeserializeObject<SearchCommandParameters>(parameters));
                        return result;
                    case Command.CreateNewFolder:
                        result.Content = CreateNewFolder(id,
                            JsonConvert.DeserializeObject<CreateNewFolderCommandParameters>(parameters));
                        return result;
                    case Command.CreateNewFile:
                        result.Content = CreateNewFile(id,
                            JsonConvert.DeserializeObject<CreateNewFileCommandParameters>(parameters));
                        return result;
                    case Command.Delete:
                        result.Content = Delete(id,
                            JsonConvert.DeserializeObject<DeleteCommandParameters>(parameters));
                        return result;
                    case Command.Rename:
                        result.Content = Rename(id,
                            JsonConvert.DeserializeObject<RenameCommandParameters>(parameters));
                        return result;
                    case Command.Zip:
                        result.Content = Zip(id,
                            JsonConvert.DeserializeObject<ZipCommandParameters>(parameters));
                        return result;
                    case Command.Unzip:
                        result.Content = UnZip(id,
                            JsonConvert.DeserializeObject<UnZipCommandParameters>(parameters));
                        return result;
                    case Command.Copy:
                    case Command.Cut:
                        result.Content = CopyCutItems(id, command,
                            JsonConvert.DeserializeObject<CopyCutCommandParameters>(parameters));
                        return result;
                    case Command.EditFile:
                        return EditFile(id,
                            JsonConvert.DeserializeObject<EditFileCommandParameters>(parameters));
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

        return Path.GetFullPath(_session.GetString(sessionKey).Trim());
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

        if (searchPattern.Contains("*"))
        {
            result.Files.AddRange(Utils.GetFiles(physicalPath, searchPattern, SearchOption.AllDirectories).OrderBy(p => p)
                .Select(p => p.GetFileDetail(physicalRootPath)));
            result.Folders.AddRange(Utils.GetDirectories(physicalPath, searchPattern, SearchOption.AllDirectories)
                .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)));
        }
        else
        {
            result.Files.AddRange(Utils.GetFiles(physicalPath, "*", SearchOption.AllDirectories).OrderBy(p => p)
                .Select(p => p.GetFileDetail(physicalRootPath)));
            result.Folders.AddRange(Utils.GetDirectories(physicalPath, "*", SearchOption.AllDirectories)
                .OrderBy(p => p).Select(p => p.GetFolderDetail(physicalRootPath)));

            result.Files.RemoveAll(p =>
                !p.FileName.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase));
            result.Folders.RemoveAll(p =>
                !p.FolderName.Contains(searchPattern, StringComparison.InvariantCultureIgnoreCase));
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

        //Compute all selected files size
        var compressionMaxSize = FileManagerComponent.ConfigStorage[id].CompressionMaxSizeMByte;
        if (compressionMaxSize > 0 || storageSizeLimit > 0)
        {
            long allFilesSize = 0;
            foreach (var item in commandParameters.Items.Select(p => p.ConvertVirtualToPhysicalPath(physicalRootPath)))
            {
                if (Directory.Exists(item))
                {
                    allFilesSize += new DirectoryInfo(item).GetFiles("*", SearchOption.AllDirectories)
                        .Sum(p => p.Length);
                }

                if (File.Exists(item))
                {
                    allFilesSize += new FileInfo(item).Length;
                }
            }

            allFilesSize = allFilesSize / 1024 / 1024;

            if (compressionMaxSize > 0)
            {
                if (compressionMaxSize < allFilesSize)
                {
                    throw new Exception(FileManagerComponent.ConfigStorage[id].Language.TooBigErrorMessage);
                }
            }

            if (storageSizeLimit > 0 && storageSizeLimit < rootFolderSize + allFilesSize)
            {
                throw new Exception(FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage);
            }
        }

        //create Zip archive
        using (Stream stream = File.Create(newZipFileName))
        using (var archive = ZipArchive.Create())
        {
            archive.DeflateCompressionLevel = (CompressionLevel)FileManagerComponent.ConfigStorage[id].CompressionLevel;

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
                LeaveStreamOpen = false,
            });
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

        //compute all selected zip files size
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

        //decompress zip archive
        foreach (var item in commandParameters.Items)
        {
            var zipArchivePhysicalPath = item.ConvertVirtualToPhysicalPath(physicalRootPath);

            if (File.Exists(zipArchivePhysicalPath))
            {
                using Stream stream = File.OpenRead(zipArchivePhysicalPath);
                using var reader = ReaderFactory.Open(stream);
                reader.WriteAllToDirectory(physicalPath, new ExtractionOptions()
                {
                    ExtractFullPath = true,
                    Overwrite = true,
                });
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
                Content = FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage, //FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage,
                StatusCode = 404 //not found
            };
        }

        if (view)
        {
            return new FileStreamResult(File.OpenRead(physicalPath), Utils.GetMimeTypeForFileExtension(physicalPath));
        }

        return new PhysicalFileResult(physicalPath, Utils.GetMimeTypeForFileExtension(physicalPath))
        {
            FileDownloadName = Path.GetFileName(physicalPath),
            EnableRangeProcessing = true
        };
    }

    private IActionResult GetFileContent(string id, string filePath)
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

        if (Utils.IsBinary(physicalPath))
        {
            return new ContentResult()
            {
                Content = Path.GetFileName(physicalPath) + FileManagerComponent.ConfigStorage[id].Language.IsNotEditableFileErrorMessage,
                StatusCode = 400 //bad Request
            };
        }

        EditViewModel viewModel = new()
        {
            Id = id,
            FileFullPath = filePath,
            FileName = Path.GetFileName(physicalPath),
            FileData = File.ReadAllText(physicalPath),
            Language = FileManagerComponent.ConfigStorage[id].Language,
        };

        var result = new ViewResult()
        {
            ViewName = "HgoFileManager/Edit",
            TempData = new TempDataDictionary(_httpContextAccessor.HttpContext, _tempDataProvider),
        };
        result.TempData["model"] = viewModel;

        return result;
    }

    private IActionResult EditFile(string id, EditFileCommandParameters commandParameters)
    {
        var storageSizeLimit = FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte;
        var fileContentSize = ASCIIEncoding.Unicode.GetByteCount(commandParameters.Data) / 1024 / 1024;
        if (storageSizeLimit > 0 && storageSizeLimit < GetRootFolderSize(id) + fileContentSize)
        {
            return new ContentResult()
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
            return new ContentResult()
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
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new
                {
                    message = FileManagerComponent.ConfigStorage[id].Language.NotFoundErrorMessage
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
        var storageSizeLimit = FileManagerComponent.ConfigStorage[id].StorageMaxSizeMByte;
        if (storageSizeLimit > 0 && storageSizeLimit < GetRootFolderSize(id) + (file.Length / 1024 / 1024))
        {
            return new ContentResult()
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.NotEnoughSpaceErrorMessage,
                StatusCode = 400
            };
        }

        var maxFileSizeToUpload = FileManagerComponent.ConfigStorage[id].MaxFileSizeToUploadMByte;
        if (maxFileSizeToUpload > 0 && maxFileSizeToUpload < (file.Length / 1024 / 1024))
        {
            return new ContentResult()
            {
                Content = FileManagerComponent.ConfigStorage[id].Language.TooBigErrorMessage,
                StatusCode = 400
            };
        }

        var acceptedFiles = FileManagerComponent.ConfigStorage[id].AcceptedFiles;
        if (!string.IsNullOrWhiteSpace(acceptedFiles))
        {
            var fileExt = Path.GetExtension(file.FileName).ToLower().Trim();
            if (!acceptedFiles.ToLower().Contains(fileExt))
            {
                return new ContentResult()
                {
                    Content = $"'{fileExt}' {FileManagerComponent.ConfigStorage[id].Language.FilesNotAcceptedErrorMessage}",
                    StatusCode = 400
                };
            }
        }

        var physicalRootPath = GetCurrentSessionPhysicalRootPath(id);
        if (string.IsNullOrWhiteSpace(physicalRootPath))
        {
            return new ContentResult()
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

        if (File.Exists(filePath) && _httpContextAccessor.HttpContext.Request.Form.TryGetValue("dzchunkindex", out StringValues chunkIndex))
        {
            if (int.TryParse(chunkIndex.ToString(), out int idx) && idx == 0)
            {
                return new ContentResult()
                {
                    Content = FileManagerComponent.ConfigStorage[id].Language.FileAlreadyExistsErrorMessage,
                    StatusCode = 400
                };
            }
        }

        using Stream fileStream = new FileStream(filePath, FileMode.Append);
        file.CopyTo(fileStream);

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