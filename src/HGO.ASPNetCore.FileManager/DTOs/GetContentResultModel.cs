using Newtonsoft.Json;

namespace HGO.ASPNetCore.FileManager.DTOs
{
    public class GetContentResultModel
    {
        public List<FolderDetail?> Folders { get; set; } = null!;
        public List<FileDetail?> Files { get; set; } = null!;
        public string CurrentPath { get; set; } = null!;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class FileDetail
    {
        public string FileName { get; set; } = null!;
        public string VirtualPath { get; set; } = null!;
        public string FileSize { get; set; } = null!;
        public string ModifiedDate { get; set; } = null!;
        public string CreateDate { get; set; } = null!;
    }

    public class FolderDetail
    {
        public string FolderName { get; set; } = null!;
        public string VirtualPath { get; set; } = null!;
        public string ModifiedDate { get; set; } = null!;
        public string CreateDate { get; set; } = null!;
    }
}
