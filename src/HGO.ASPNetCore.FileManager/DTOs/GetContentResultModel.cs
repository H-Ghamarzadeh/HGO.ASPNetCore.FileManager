using Newtonsoft.Json;

namespace HGO.ASPNetCore.FileManager.DTOs
{
    public class GetContentResultModel
    {
        public List<FolderDetail> Folders { get; set; } = new List<FolderDetail>();
        public List<FileDetail> Files { get; set; } = new List<FileDetail>();
        public string Error { get; set; }
        public string CurrentPath { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class FileDetail
    {
        public string FileName { get; set; }
        public string VirtualPath { get; set; }
        public string FileSize { get; set; }
        public string ModifiedDate { get; set; }
        public string CreateDate { get; set; }
    }

    public class FolderDetail
    {
        public string FolderName { get; set; }
        public string VirtualPath { get; set; }
        public string ModifiedDate { get; set; }
        public string CreateDate { get; set; }
    }
}
