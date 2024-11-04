namespace HGO.ASPNetCore.FileManager.ViewComponentsModel
{
    public class FileManagerModel
    {
        public string Id { get; set; } = string.Empty;
        public string RootFolder { get; set; } = string.Empty;
        public string ApiEndPoint { get; set; } = string.Empty;
        public FileManagerConfig Config { get; set; } = new FileManagerConfig();
    }
}
