namespace HGO.ASPNetCore.FileManager.ViewComponentsModel.LanguageModels
{
    public class CustomLanguage : LanguageBase, ILanguage
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string Browse { get; set; } = "*Not Defined*";
        public string Copy { get; set; } = "*Not Defined*";
        public string Cut { get; set; } = "*Not Defined*";
        public string Paste { get; set; } = "*Not Defined*";
        public string Rename { get; set; } = "*Not Defined*";
        public string Edit { get; set; } = "*Not Defined*";
        public string Save { get; set; } = "*Not Defined*";
        public string Delete { get; set; } = "*Not Defined*";
        public string CreateNewFolder { get; set; } = "*Not Defined*";
        public string NewFolderPlaceHolder { get; set; } = "*Not Defined*";
        public string CreateNewFile { get; set; } = "*Not Defined*";
        public string NewFilePlaceHolder { get; set; } = "*Not Defined*";
        public string View { get; set; } = "*Not Defined*";
        public string Download { get; set; } = "*Not Defined*";
        public string Search { get; set; } = "*Not Defined*";
        public string Zip { get; set; } = "*Not Defined*";
        public string Unzip { get; set; } = "*Not Defined*";
        public string GetFolderContent { get; set; } = "*Not Defined*";
        public string GetFileContent { get; set; } = "*Not Defined*";
        public string Upload { get; set; } = "*Not Defined*";
        public string ToggleView { get; set; } = "*Not Defined*";
        public string Reload { get; set; } = "*Not Defined*";
        public string Breadcrumb { get; set; } = "*Not Defined*";
        public string FoldersTree { get; set; } = "*Not Defined*";
        public string MenuBar { get; set; } = "*Not Defined*";
        public string ContextMenu { get; set; } = "*Not Defined*";
        public string FilePreview { get; set; } = "*Not Defined*";
        public string CreateDate { get; set; } = "*Not Defined*";
        public string ModifiedDate { get; set; } = "*Not Defined*";
        public string FileName { get; set; } = "*Not Defined*";
        public string FolderName { get; set; } = "*Not Defined*";
        public string Size { get; set; } = "*Not Defined*";
        public string Back { get; set; } = "*Not Defined*";
        public string Up { get; set; } = "*Not Defined*";
        public string Close { get; set; } = "*Not Defined*";
        public string EncryptionStatus { get; set; } = "*Not Defined*";
        public string IsEncrypted { get; set; } = "*Not Defined*";
        public string NotEncrypted { get; set; } = "*Not Defined*";
        public string EncryptedFileExplanation { get; set; } = "*Not Defined*";

        // Notifications and Messages
        public string NoItemsSelectedMessage { get; set; } = "*Not Defined*";
        public string EnterNewFolderNameMessage { get; set; } = "*Not Defined*";
        public string EnterNewFileNameMessage { get; set; } = "*Not Defined*";
        public string DeleteConfirmationMessage { get; set; } = "*Not Defined*";
        public string RenameMessage { get; set; } = "*Not Defined*";
        public string ItemAlreadyExistMessage { get; set; } = "*Not Defined*";
        public string ZipFileNameMessage { get; set; } = "*Not Defined*";
        public string OverrideConfirmationMessage { get; set; } = "*Not Defined*";
        public string UnknownCommandErrorMessage { get; set; } = "*Not Defined*";
        public string InvalidRootPathErrorMessage { get; set; } = "*Not Defined*";
        public string NotEnoughSpaceErrorMessage { get; set; } = "*Not Defined*";
        public string TooBigErrorMessage { get; set; } = "*Not Defined*";
        public string ActionDisabledErrorMessage { get; set; } = "*Not Defined*";
        public string NotFoundErrorMessage { get; set; } = "*Not Defined*";
        public string IsNotEditableFileErrorMessage { get; set; } = "*Not Defined*";
        public string FilesNotAcceptedErrorMessage { get; set; } = "*Not Defined*";
        public string FileAlreadyExistsErrorMessage { get; set; } = "*Not Defined*";


        // Dropzone-related messages
        public string DictDefaultMessage { get; set; } = "*Not Defined*";
        public string DictFallbackMessage { get; set; } = "*Not Defined*";
        public string DictFallbackText { get; set; } = "*Not Defined*";
        public string DictFileTooBig { get; set; } = "*Not Defined*";
        public string DictInvalidFileType { get; set; } = "*Not Defined*";
        public string DictResponseError { get; set; } = "*Not Defined*";
        public string DictCancelUpload { get; set; } = "*Not Defined*";
        public string DictCancelUploadConfirmation { get; set; } = "*Not Defined*";
        public string DictRemoveFile { get; set; } = "*Not Defined*";
        public string DictMaxFilesExceeded { get; set; } = "*Not Defined*";
    }
}