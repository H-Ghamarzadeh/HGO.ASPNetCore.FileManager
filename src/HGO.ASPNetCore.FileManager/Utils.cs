using HGO.ASPNetCore.FileManager.DTOs;
using Microsoft.AspNetCore.StaticFiles;

namespace HGO.ASPNetCore.FileManager
{
    internal static class Utils
    {
        public static string TrimStart(this string target, string trimString, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(trimString))
            {
                return target.TrimStart();
            }

            string result = target;
            while (result.StartsWith(trimString, comparison))
            {
                result = result.Substring(trimString.Length);
            }

            return result;
        }

        public static string TrimEnd(this string target, string trimString, StringComparison comparison = StringComparison.InvariantCultureIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(trimString))
            {
                return target.TrimEnd();
            }

            string result = target;
            while (result.EndsWith(trimString, comparison))
            {
                result = result.Substring(0, result.Length - trimString.Length);
            }

            return result;
        }

        public static string ConvertPhysicalToVirtualPath(this string physicalPath, string physicalRootPath, string rootName = "Root")
        {
            var rootPath = physicalRootPath.TrimEnd(Path.DirectorySeparatorChar);
            var virtualPath = rootName.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + physicalPath.TrimStart(rootPath).TrimStart(Path.DirectorySeparatorChar);
            return virtualPath.Replace(Path.DirectorySeparatorChar, '\\');
        }

        public static string ConvertVirtualToPhysicalPath(this string virtualPath, string physicalRootPath, string rootName = "Root")
        {
            virtualPath = virtualPath
                //Replace '\' char with actual directory separator char (based on running OS)
                .Replace('\\', Path.DirectorySeparatorChar)
                //Remove virtual root from start of path
                .TrimStart(rootName.TrimEnd(Path.DirectorySeparatorChar))
                .TrimStart(Path.DirectorySeparatorChar);

            return Path.Combine(physicalRootPath, virtualPath);
        }

        public static void CopyDirectory(string src, string dest, bool overWrite)
        {
            var srcDirInfo = new DirectoryInfo(src);
            if (!Directory.Exists(Path.Combine(dest, srcDirInfo.Name)))
            {
                Directory.CreateDirectory(Path.Combine(dest, srcDirInfo.Name));
            }

            dest = Path.Combine(dest, srcDirInfo.Name);

            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(src, "*",
                         SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(src, dest));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(src, "*.*",
                         SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(src, dest), overWrite);
        }

        public static string GetMimeTypeForFileExtension(string filePath)
        {
            const string defaultContentType = "application/octet-stream";

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(filePath, out string contentType))
            {
                contentType = defaultContentType;
            }

            return contentType;
        }

        public static bool IsBinary(string filePath, int requiredConsecutiveNul = 5)
        {
            const int charsToCheck = 8000;
            const char nulChar = '\0';

            int nulCount = 0;

            using (var streamReader = new StreamReader(filePath))
            {
                for (var i = 0; i < charsToCheck; i++)
                {
                    if (streamReader.EndOfStream)
                        return false;

                    if ((char)streamReader.Read() == nulChar)
                    {
                        nulCount++;

                        if (nulCount >= requiredConsecutiveNul)
                            return true;
                    }
                    else
                    {
                        nulCount = 0;
                    }
                }
            }

            return false;
        }

        public static FileDetail? GetFileDetail(this string filePath, string physicalRootPath, string rootName = "Root")
        {
            if (!File.Exists(filePath)) return null;

            var fileDetail = new FileDetail();

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            var fileInfo = new FileInfo(filePath);
            double len = fileInfo.Length;
            var order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            fileDetail.FileSize = $"{len:0.##} {sizes[order]}";
            fileDetail.CreateDate = fileInfo.CreationTime.ToString("yyyy MMM dd - HH:mm");
            fileDetail.ModifiedDate = fileInfo.LastWriteTime.ToString("yyyy MMM dd - HH:mm");
            fileDetail.FileName = fileInfo.Name;
            fileDetail.VirtualPath = filePath.ConvertPhysicalToVirtualPath(physicalRootPath, rootName);

            return fileDetail;
        }

        public static FolderDetail? GetFolderDetail(this string folderPath, string physicalRootPath, string rootName = "Root")
        {
            if (!Directory.Exists(folderPath)) return null;

            var folderDetail = new FolderDetail();

            var directoryInfo = new DirectoryInfo(folderPath);

            folderDetail.CreateDate = directoryInfo.CreationTime.ToString("yyyy MMM dd - HH:mm");
            folderDetail.ModifiedDate = directoryInfo.LastWriteTime.ToString("yyyy MMM dd - HH:mm");
            folderDetail.FolderName = directoryInfo.Name;
            folderDetail.VirtualPath = folderPath.ConvertPhysicalToVirtualPath(physicalRootPath, rootName);

            return folderDetail;
        }

        public static List<string> GetFiles(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetFiles(path, searchPattern, searchOption).Where(p=> !new FileInfo(p).Attributes.HasFlag(FileAttributes.Hidden)).OrderBy(p=> p).ToList();
        }

        public static List<string> GetDirectories(string path, string searchPattern, SearchOption searchOption)
        {
            return Directory.GetDirectories(path, searchPattern, searchOption).Where(p => !new DirectoryInfo(p).Attributes.HasFlag(FileAttributes.Hidden)).OrderBy(p => p).ToList();
        }
    }
}
