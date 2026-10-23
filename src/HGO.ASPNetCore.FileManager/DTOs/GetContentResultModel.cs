using Newtonsoft.Json;

namespace HGO.ASPNetCore.FileManager.DTOs
{
    /// <summary>Represents the current directory contents returned by the file manager.</summary>
    public class GetContentResultModel
    {
        /// <summary>Gets or sets the folders returned by the request. An item can be null if it disappears while the result is being assembled.</summary>
        public List<FolderDetail?> Folders { get; set; } = new List<FolderDetail?>();

        /// <summary>Gets or sets the files returned by the request. An item can be null if it disappears while the result is being assembled.</summary>
        public List<FileDetail?> Files { get; set; } = new List<FileDetail?>();

        /// <summary>Gets or sets the virtual path of the current directory.</summary>
        public string CurrentPath { get; set; } = null!;

        /// <summary>Gets or sets the count of failed/incomplete uploads in the current directory.</summary>
        public int FailedUploadsCount { get; set; }

        /// <summary>Serializes this result as JSON.</summary>
        /// <returns>This result serialized as JSON.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>Describes a file shown by the file manager.</summary>
    public class FileDetail
    {
        /// <summary>Gets or sets the file name.</summary>
        public string FileName { get; set; } = null!;

        /// <summary>Gets or sets the file's virtual path.</summary>
        public string VirtualPath { get; set; } = null!;

        /// <summary>Gets or sets the formatted file size.</summary>
        public string FileSize { get; set; } = null!;

        /// <summary>Gets or sets the formatted last-modified date.</summary>
        public string ModifiedDate { get; set; } = null!;

        /// <summary>Gets or sets the formatted creation date.</summary>
        public string CreateDate { get; set; } = null!;
    }

    /// <summary>Describes a folder shown by the file manager.</summary>
    public class FolderDetail
    {
        /// <summary>Gets or sets the folder name.</summary>
        public string FolderName { get; set; } = null!;

        /// <summary>Gets or sets the folder's virtual path.</summary>
        public string VirtualPath { get; set; } = null!;

        /// <summary>Gets or sets the formatted last-modified date.</summary>
        public string ModifiedDate { get; set; } = null!;

        /// <summary>Gets or sets the formatted creation date.</summary>
        public string CreateDate { get; set; } = null!;
    }
}
