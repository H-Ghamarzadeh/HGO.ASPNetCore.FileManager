using System;
using HGO.ASPNetCore.FileManager.ViewComponentsModel.LanguageModels;

namespace HGO.ASPNetCore.FileManager.ViewComponentsModel.LanguageModels.BuiltIn
{
    /// <summary>
    /// English language
    /// </summary>
    public class EnglishLanguage : LanguageBase, ILanguage
    {
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public string Browse { get; } = "Browse";
        public string Copy { get; } = "Copy";
        public string Cut { get; } = "Cut";
        public string Paste { get; } = "Paste";
        public string Rename { get; } = "Rename";
        public string Edit { get; } = "Edit";
        public string Save { get; } = "Save";
        public string Delete { get; } = "Delete";
        public string CreateNewFolder { get; } = "Create New Folder";
        public string CreateNewFile { get; } = "Create New File";
        public string View { get; } = "View";
        public string Download { get; } = "Download";
        public string Search { get; } = "Search";
        public string Zip { get; } = "Zip";
        public string Unzip { get; } = "Unzip";
        public string GetFolderContent { get; } = "Get Folder Content";
        public string GetFileContent { get; } = "Get File Content";
        public string Upload { get; } = "Upload";
        public string ToggleView { get; } = "Toggle View";
        public string Reload { get; } = "Reload";
        public string Breadcrumb { get; } = "Breadcrumb";
        public string FoldersTree { get; } = "Folders Tree";
        public string MenuBar { get; } = "Menu Bar";
        public string ContextMenu { get; } = "Context Menu";
        public string FilePreview { get; } = "File Preview";
        public string NewFolderPlaceHolder { get; } = "New Folder";
        public string NewFilePlaceHolder { get; } = "New Text.txt";
        public string NoItemsSelectedMessage { get; } = "Please select your desired item(s).";
        public string CreateDate { get; } = "Create Date";
        public string ModifiedDate { get; } = "Modified Date";
        public string FileName { get; } = "File Name";
        public string FolderName { get; } = "Folder Name";
        public string Size { get; } = "Size";
        public string EncryptedSize { get; } = "Encrypted Size";
        public string Back { get; } = "Back";
        public string Up { get; } = "Up";
        public string Close { get; } = "Close";
        public string EncryptionStatus { get; set; } = "Encryption Status";
        public string IsEncrypted { get; set; } = "Encrypted";
        public string Unencrypted { get; set; } = "Unencrypted";
        public string EncryptedFileExplanation { get; set; } = "Encrypted files can only be modified through this interface and cannot be altered by the file manager. Even if direct access to the encrypted files is obtained, individuals without access to this interface will not be able to view the contents of the files.";
        public string Encrypt { get; set; } = "Encrypt";
        public string Decrypt { get; set; } = "Decrypt";
        public string EnterNewFolderNameMessage { get; } = "Please enter folder name:";
        public string EnterNewFileNameMessage { get; } = "Please enter your desired file name:";
        public string DeleteConfirmationMessage { get; } = "Are you sure you want to delete selected items?";
        public string RenameMessage { get; } = "Please enter new name:";
        public string ItemAlreadyExistMessage { get; } = "already exist, do you want to overwrite?";
        public string ZipFileNameMessage { get; } = "Please enter Zip file name:";
        public string OverrideConfirmationMessage { get; } = "Some item(s) already exist, do you want to overwrite?";

        public string UnknownCommandErrorMessage { get; } = "Unknown command.";
        public string InvalidRootPathErrorMessage { get; } = "Invalid root path.";
        public string NotEnoughSpaceErrorMessage { get; } = "Not enough space.";
        public string TooBigErrorMessage { get; } = "File is too big."!;
        public string ActionDisabledErrorMessage { get; } = "Action is disabled.";
        public string NotFoundErrorMessage { get; } = "File not found.";
        public string IsNotEditableFileErrorMessage { get; } = "File is not editable.";
        public string FilesNotAcceptedErrorMessage { get; } = "Files not accepted.";
        public string FileAlreadyExistsErrorMessage { get; } = "File already exists.";

        // Dropzone-related messages in English
        public string DictDefaultMessage { get; } = "Drag files here or click to upload";
        public string DictFallbackMessage { get; } = "Your browser does not support file uploads.";
        public string DictFallbackText { get; } = "Please use a fallback form to upload your files.";
        public string DictFileTooBig { get; } = "File is too big ({{filesize}}MiB). Max filesize: {{maxFilesize}}MiB.";
        public string DictInvalidFileType { get; } = "You cannot upload files of this type.";
        public string DictResponseError { get; } = "Server responded with {{statusCode}} code.";
        public string DictCancelUpload { get; } = "Cancel upload";
        public string DictCancelUploadConfirmation { get; } = "Are you sure you want to cancel this upload?";
        public string DictRemoveFile { get; } = "Remove file";
        public string DictMaxFilesExceeded { get; } = "You cannot upload any more files.";
    }
}