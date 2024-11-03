using HGO.ASPNetCore.FileManager.Enums;
using HGO.ASPNetCore.FileManager.Models.LanguageModels;
using HGO.ASPNetCore.FileManager.Models.LanguageModels.BuiltIn;

namespace HGO.ASPNetCore.FileManager.ViewComponentsModel
{
    public class FileManagerConfig
    {
        private byte _compressionLevel = 6;

        /// <summary>
        /// The maximum storage space (in megabytes) that the user can use
        /// </summary>
        public long StorageMaxSizeMByte { get; set; } = 1024;

        /// <summary>
        /// Gets or sets the maximum allowed size to compress (per action). If not set 0 (zero), there will be no size limit for compression.
        /// </summary>
        public long CompressionMaxSizeMByte { get; set; } = 256;

        /// <summary>
        /// The maximum filesize (in megabytes) that is allowed to be uploaded.
        /// </summary>
        public long MaxFileSizeToUploadMByte { get; set; } = 256;

        /// <summary>
        /// Whether you want files to be uploaded in chunks to your server. 
        /// </summary>
        public bool Chunking { get; set; } = true;

        /// <summary>
        /// If chunking is true, then this defines the chunk size in bytes.
        /// </summary>
        public long ChunkSizeByte { get; set; } = 10000000;

        /// <summary>
        /// Whether a chunk should be retried if it fails.
        /// </summary>
        public bool RetryChunks { get; set; } = true;

        /// <summary>
        /// If retryChunks is true, how many times should it be retried.
        /// </summary>
        public int RetryChunksLimit { get; set; } = 3;

        /// <summary>
        /// How many file uploads to process in parallel.
        /// </summary>
        public int ParallelUploads { get; set; } = 1;

        /// <summary>
        /// Allowed file extensions to upload (comma separated). If not set, there will be no file format limit for upload.
        /// e.g.: ".pdf,.png"
        /// </summary>
        public string AcceptedFiles { get; set; } = "";

        /// <summary>
        /// Disabled functions list:
        /// "Search", "CreateNewFolder", "CreateNewFile", "Delete", "Rename", "Zip",
        /// "Unzip", "Copy", "Cut", "EditFile", "Download", "GetFileContent", "Upload",
        /// "ToggleView", "Browse", "Reload", "Breadcrumb", "FoldersTree", "MenuBar",
        /// "ContextMenu", "FilePreview", "View"
        /// </summary>
        public HashSet<Command> DisabledFunctions { get; set; } = new HashSet<Command>();

        /// <summary>
        /// Compression Level: from 0 (fastest) to 9 (best compression)
        /// </summary>
        public byte CompressionLevel
        {
            get => _compressionLevel;
            set
            {
                if (value < 0) { _compressionLevel = 0; }
                else if (value > 9) { _compressionLevel = 9; }
                else { _compressionLevel = value; }
            }
        }

        /// <summary>
        /// Language For UI Default is (Built-In) new EnglishLanguage();
        /// Built-In alternative: TurkishLanguage();
        /// or CustomLanguage() for setting custom properties...
        /// </summary>
        public ILanguage Language { get; set; } = new EnglishLanguage();



    }
}
