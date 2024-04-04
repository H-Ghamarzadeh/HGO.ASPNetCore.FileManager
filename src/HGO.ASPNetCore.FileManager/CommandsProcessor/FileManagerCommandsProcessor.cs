using HGO.ASPNetCore.FileManager.CommandsProcessor.Dto;
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
        try
        {
            var result = new ContentResult()
            {
                StatusCode = 200
            };
            
            if (command.ToLower() == "getFolderContent".ToLower())
            {
                result.Content = GetContent(id, parameters);
                return result;
            }
            else if (command.ToLower() == "Search".ToLower())
            {
                var parameter = JsonConvert.DeserializeObject<SearchCommandParameters>(parameters);
                result.Content = GetContent(id, parameter.Path, parameter.Query);
                return result;
            }
            else if (command.ToLower() == "CreateNewFolder".ToLower())
            {
                result.Content = CreateNewFolder(id, JsonConvert.DeserializeObject<CreateNewFolderCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "CreateNewFile".ToLower())
            {
                result.Content = CreateNewFile(id, JsonConvert.DeserializeObject<CreateNewFileCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "DeleteItems".ToLower())
            {
                result.Content = DeleteItems(id, JsonConvert.DeserializeObject<DeleteItemsCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "RenameItems".ToLower())
            {
                result.Content = RenameItems(id, JsonConvert.DeserializeObject<RenameItemsCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "ZipItems".ToLower())
            {
                result.Content = ZipItems(id, JsonConvert.DeserializeObject<ZipItemsCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "ExtractItems".ToLower())
            {
                result.Content = ExtractItems(id, JsonConvert.DeserializeObject<ExtractItemsCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "DownloadItem".ToLower())
            {
                return DownloadItem(id, parameters);
            }
            else if (command.ToLower() == "ShowFileContent".ToLower())
            {
                return ShowFileContent(id, parameters);
            }
            else if (command.ToLower() == "EditFileContent".ToLower())
            {
                result.Content = EditFileContent(id, JsonConvert.DeserializeObject<EditFileCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "Upload".ToLower())
            {
                return await Upload(id, parameters, file);
            }
            else if (command.ToLower() == "Copy".ToLower() || command.ToLower() == "Cut".ToLower())
            {
                result.Content = CopyCutItems(id, command.ToLower(), JsonConvert.DeserializeObject<CopyCutCommandParameters>(parameters));
                return result;
            }
            else if (command.ToLower() == "FilePreview".ToLower())
            {
                return FilePreview(id, parameters);
            }

            return new ContentResult(){
                Content = JsonConvert.SerializeObject(new
                {
                    error = "Unknown command!"
                }),
                StatusCode = 500
            };
        }
        catch (Exception e)
        {
            return new ContentResult()
            {
                Content = JsonConvert.SerializeObject(new
                {
                    error = e.Message
                }),
                StatusCode = 500
            };
        }
    }

    public string? GetCurrentUserRootPath(string id)
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

    private string GetContent(string id, string path, string searchPattern = "*.*")
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        path = path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        if (string.IsNullOrWhiteSpace(searchPattern))
        {
            searchPattern = "*.*";
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            dirs.AddRange(Directory.GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly).Select(p=> p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)  
        {
            error = ex.Message;
            dirs.Clear();
            files.Clear();
            path = rootPath;

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        
        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath).Replace(Path.DirectorySeparatorChar, '\\'),
            dirs,
            files,
            error
        });
    }

    private string CreateNewFolder(string id, CreateNewFolderCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            var newFolder = Path.Combine(path, commandParameters.FolderName);
            var tmp = newFolder;
            var counter = 1;
            while (Directory.Exists(tmp))
            {
                tmp = newFolder+ $" ({counter})";
                counter++;
            }
            newFolder = tmp;
            Directory.CreateDirectory(newFolder);

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = "Invalid folder name!";
            dirs.Clear();
            files.Clear();
            path = rootPath;

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }
    
    private string CreateNewFile(string id, CreateNewFileCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            var newFile = Path.Combine(path, commandParameters.FileName);
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

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = "Invalid folder name!";
            dirs.Clear();
            files.Clear();
            path = rootPath;

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }

    private string DeleteItems(string id, DeleteItemsCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            foreach (var item in commandParameters.Items)
            {
                var physicalPath = item.ConvertVirtualToPhysicalPath(rootPath);

                if (Directory.Exists(physicalPath))
                {
                    Directory.Delete(physicalPath, true);
                }
                else if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                }
            }

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            dirs.Clear();
            files.Clear();

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }

    private string RenameItems(string id, RenameItemsCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            //Move Files/Folders
            int dirCounter = 0;
            int fileCounter = 0;
            foreach (var item in commandParameters.Items)
            {
                var physicalPath = item.ConvertVirtualToPhysicalPath(rootPath);

                if (Directory.Exists(physicalPath))
                {
                    var newDir = Path.Combine(new DirectoryInfo(physicalPath).Parent.FullName,
                        commandParameters.NewName) + (dirCounter > 0 ? $" ({dirCounter})" : "");
                    dirCounter++;

                    if (newDir.ToLower().Trim() == physicalPath.ToLower().Trim())
                    {
                        continue;
                    }

                    Directory.Move(physicalPath, newDir);
                }

                if (File.Exists(physicalPath))
                {
                    var ext = Path.GetExtension(commandParameters.NewName);
                    var newFile = Path.Combine(new FileInfo(physicalPath).DirectoryName,
                        commandParameters.NewName);
                    if (fileCounter > 0)
                    {
                        newFile = newFile.TrimEnd(ext) + $" ({fileCounter})" + ext;
                    }

                    fileCounter++;

                    if (newFile.ToLower().Trim() == physicalPath.ToLower().Trim())
                    {
                        continue;
                    }

                    File.Move(physicalPath, newFile, true);
                }
            }

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            dirs.Clear();
            files.Clear();

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }

    private string ZipItems(string id, ZipItemsCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            using (Stream stream = File.Create(Path.Combine(path, commandParameters.FileName.TrimStart(Path.DirectorySeparatorChar).TrimEnd(".zip")+".zip")))
            using (var archive = ZipArchive.Create())
            {
                foreach (var item in commandParameters.Items.Select(p => p.ConvertVirtualToPhysicalPath(rootPath)))
                {
                    if (Directory.Exists(item))
                    {
                        foreach (var file in Directory.GetFiles(item, "*.*", SearchOption.AllDirectories))
                        {
                            archive.AddEntry(file.TrimStart(path), file);
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

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            dirs.Clear();
            files.Clear();

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }
    
    private string ExtractItems(string id, ExtractItemsCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            foreach (var item in commandParameters.Items)
            {
                var physicalPath = item.ConvertVirtualToPhysicalPath(rootPath);

                if (File.Exists(physicalPath))
                {
                    try
                    {
                        using (Stream stream = File.OpenRead(physicalPath))
                        using (var reader = ReaderFactory.Open(stream))
                        {
                            reader.WriteAllToDirectory(path, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true,
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        error += $"Error: {e.Message}\r\n";
                    }
                }
                else
                {
                    error += $"Invalid file name: {item}\r\n";
                }
            }

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            dirs.Clear();
            files.Clear();

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }

    private IActionResult DownloadItem(string id, string filePath)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return new StatusCodeResult(404); }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(rootPath);
        if (!physicalPath.ToLower().StartsWith(rootPath.ToLower()) || !System.IO.File.Exists(physicalPath))
        {
            return new StatusCodeResult(404);
        }
        return new PhysicalFileResult(physicalPath, Utils.GetMimeTypeForFileExtension(physicalPath))
        {
            FileDownloadName = Path.GetFileName(physicalPath),
            EnableRangeProcessing = true
        };
    }

    private IActionResult ShowFileContent(string id, string filePath)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return new StatusCodeResult(404); }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(rootPath);
        if (!physicalPath.ToLower().StartsWith(rootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new StatusCodeResult(404); //File Not Found
        }

        if (Utils.IsBinary(physicalPath))
        {
            return new ContentResult()
            {
                Content = Path.GetFileName(physicalPath) + " is not editable file!",
                StatusCode = 400 //Bad Request
            };
        }

        var error = string.Empty;
        var result = new ViewResult()
        {
            ViewName = "HgoFileManager/Edit",
            TempData = new TempDataDictionary(_httpContextAccessor.HttpContext, _tempDataProvider)
        };
        try
        {
            try
            {
                result.TempData["Id"]  = id;
                result.TempData["FileFullPath"] = filePath;
                result.TempData["FileName"] = Path.GetFileName(physicalPath);
                result.TempData["FileData"] = File.ReadAllText(physicalPath);
            }
            catch (Exception e)
            {
                error += $"Error: {e.Message}\r\n";
            }
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        return result;
    }

    private string EditFileContent(string id, EditFileCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { message = "Something went wrong!" }); }

        var physicalPath = commandParameters.FilePath.ConvertVirtualToPhysicalPath(rootPath);
        if (!physicalPath.ToLower().StartsWith(rootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return JsonConvert.SerializeObject(new { message = "Something went wrong!" });
        }

        var error = string.Empty;
        try
        {
            File.WriteAllText(physicalPath, commandParameters.Data);
            return JsonConvert.SerializeObject(new { message = "OK" });
        }
        catch (Exception e)
        {
            error += $"Error: {e.Message}\r\n";
        }

        return JsonConvert.SerializeObject(new { message = error });
    }
    private string CopyCutItems(string id, string action, CopyCutCommandParameters commandParameters)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return JsonConvert.SerializeObject(new { error = "Something went wrong!" }); }

        var path = commandParameters.Path.ConvertVirtualToPhysicalPath(rootPath);

        if (!path.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(path))
        {
            path = rootPath;
        }

        var dirs = new List<string>();
        var files = new List<string>();
        var error = string.Empty;
        try
        {
            foreach (var item in commandParameters.Items)
            {
                var physicalPath = item.ConvertVirtualToPhysicalPath(rootPath);

                if (File.Exists(physicalPath))
                {
                    try
                    {
                        if (action.Trim().ToLower() == "copy")
                        {
                            File.Copy(physicalPath, Path.Combine(path , Path.GetFileName(physicalPath)), true);
                        }
                        else if (action.Trim().ToLower() == "cut")
                        {
                            File.Move(physicalPath, Path.Combine(path, Path.GetFileName(physicalPath)), true);
                        }
                    }
                    catch (Exception e)
                    {
                        error += $"Error: {e.Message}\r\n";
                    }
                }
                else if (Directory.Exists(physicalPath))
                {
                    try
                    {
                        Utils.CopyDirectory(physicalPath, path, true);
                        if (action.Trim().ToLower() == "cut")
                        {
                            Directory.Delete(physicalPath, true);
                        }
                    }
                    catch (Exception e)
                    {
                        error += $"Error: {e.Message}\r\n";
                    }
                }
            }

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }
        catch (Exception ex)
        {
            error = ex.Message;
            dirs.Clear();
            files.Clear();

            dirs.AddRange(Directory.GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => p.ConvertPhysicalToVirtualPath(rootPath)).OrderBy(p => p));
            files.AddRange(Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Select(p => Path.GetFileName(p)).OrderBy(p => p));
        }

        return JsonConvert.SerializeObject(new
        {
            path = path.ConvertPhysicalToVirtualPath(rootPath),
            dirs,
            files,
            error
        });
    }

    private async Task<IActionResult> Upload(string id, string path, IFormFile file)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return new StatusCodeResult(500); }

        var physicalPath = path.ConvertVirtualToPhysicalPath(rootPath);

        if (!physicalPath.ToLower().StartsWith(rootPath.ToLower()) || !Directory.Exists(physicalPath))
        {
            physicalPath = rootPath;
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

            await using Stream fileStream = new FileStream(filePath, FileMode.Append);
            await file.CopyToAsync(fileStream);
        }

        return new OkResult();
    }
    
    private IActionResult FilePreview(string id, string filePath)
    {
        var rootPath = GetCurrentUserRootPath(id);
        if (string.IsNullOrWhiteSpace(rootPath)) { return new StatusCodeResult(404); }

        var physicalPath = filePath.ConvertVirtualToPhysicalPath(rootPath);
        if (!physicalPath.ToLower().StartsWith(rootPath.ToLower()) || !File.Exists(physicalPath))
        {
            return new StatusCodeResult(404);
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
                return new RedirectResult("/hgofilemanager/images/file.png") {Permanent = true};
        }
    }
}