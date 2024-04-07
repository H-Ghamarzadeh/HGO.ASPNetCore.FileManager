namespace HGO.ASPNetCore.FileManager.ViewComponentsModel
{
    public class FileManagerModel
    {
        public string Id { get; set; }
        public string RootFolder { get; set; }
        public string ApiEndPoint { get; set; }
        public FileManagerConfig Config { get; set; } = new FileManagerConfig();
    }
}
