namespace HGO.ASPNetCore.FileManager.ViewComponentsModel
{
    public class FileManagerModel
    {
        public required string Id { get; set; }
        public required string RootFolder { get; set; }
        public required string ApiEndPoint { get; set; }
        public FileManagerConfig Config { get; set; } = new FileManagerConfig();
    }
}
