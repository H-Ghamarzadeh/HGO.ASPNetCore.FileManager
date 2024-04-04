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
    }
}
