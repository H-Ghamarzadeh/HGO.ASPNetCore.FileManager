namespace HGO.ASPNetCore.FileManager.ViewComponentsModel
{
    public class FileManagerConfig
    {
        private byte _compressionLevel = 6;
        public long StorageMaxSizeMByte { get; set; } = 1024;
        /*
         * Gets or sets the maximum allowed size to compress (per action). If not set 0 (zero), there will be no size limit for compression.
         */
        public long CompressionMaxSizeMByte { get; set; } = 256;

        /*
         * The maximum filesize (in mega bytes) that is allowed to be uploaded.
         */
        public long MaxFileSizeToUploadMByte { get; set; } = 256;
        /*
         * Whether you want files to be uploaded in chunks to your server. 
         */
        public bool Chunking { get; set; } = true;
        /*
         * If chunking is true, then this defines the chunk size in bytes.
         */
        public long ChunkSizeByte { get; set; } = 10000000;
        /*
         * Whether a chunk should be retried if it fails.
         */
        public bool RetryChunks { get; set; } = true;
        /*
         * If retryChunks is true, how many times should it be retried.
         */
        public int RetryChunksLimit { get; set; } = 3;
        /*
         * How many file uploads to process in parallel.
         */
        public int ParallelUploads { get; set; } = 1;
        /*
         * Allowed file extensions to upload. If not set, there will be no file format limit for upload.
         */
        public string AcceptedFiles { get; set; } = ""; //.pdf,.png

        public List<string> DisabledFunctions { get; set; } = new List<string>();
        public byte CompressionLevel
        {
            get => _compressionLevel;
            set
            {
                if (value < 0) { _compressionLevel = 0; }
                else if (value > 9) { _compressionLevel = 9; }
                else { _compressionLevel = value; }
            }
        } //0 ~ 9
    }
}
