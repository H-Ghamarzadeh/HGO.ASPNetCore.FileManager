using Newtonsoft.Json;

namespace HGO.ASPNetCore.FileManager.DTOs
{
    public class GetContentResultModel
    {
        public List<FolderDetail?> Folders { get; set; } = [];
        public List<FileDetail?> Files { get; set; } = [];
        public string CurrentPath { get; set; } = string.Empty;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class FileDetail
    {
        public string FileName { get; set; } = string.Empty;
        public string VirtualPath { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string ModifiedDate { get; set; } = string.Empty;
        public string CreateDate { get; set; } = string.Empty;
        public bool IsEncrypted { get; set; } = false;
    }

    public class FolderDetail
    {
        public string FolderName { get; set; } = string.Empty;
        public string VirtualPath { get; set; } = string.Empty;
        public string ModifiedDate { get; set; } = string.Empty;
        public string CreateDate { get; set; } = string.Empty;
    }
}
