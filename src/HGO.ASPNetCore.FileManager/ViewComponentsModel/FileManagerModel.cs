namespace HGO.ASPNetCore.FileManager.ViewComponentsModel
{
    /// <summary>
    /// Provides the configuration and location information for a file-manager view component.
    /// </summary>
    public class FileManagerModel
    {
        /// <summary>Gets or sets the unique identifier for this file-manager instance.</summary>
        public string Id { get; set; } = null!;

        /// <summary>Gets or sets the physical root folder exposed by the file manager.</summary>
        public string RootFolder { get; set; } = null!;

        /// <summary>Gets or sets the endpoint that receives file-manager commands.</summary>
        public string ApiEndPoint { get; set; } = null!;

        /// <summary>Gets or sets the file-manager behavior and size configuration.</summary>
        public FileManagerConfig Config { get; set; } = new FileManagerConfig();
    }
}
