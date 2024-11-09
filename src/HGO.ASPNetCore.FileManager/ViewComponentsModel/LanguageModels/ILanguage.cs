namespace HGO.ASPNetCore.FileManager.ViewComponentsModel.LanguageModels
{
    /// <summary>
    /// Defines language-specific text for various UI elements and messages in the file manager.
    /// </summary>
    public interface ILanguage
    {
        /// <summary>Text for browsing files.</summary>
        string Browse { get; }

        /// <summary>Text for copying files or folders.</summary>
        string Copy { get; }

        /// <summary>Text for cutting files or folders.</summary>
        string Cut { get; }

        /// <summary>Text for pasting files or folders.</summary>
        string Paste { get; }

        /// <summary>Text for renaming files or folders.</summary>
        string Rename { get; }

        /// <summary>Text for editing files.</summary>
        string Edit { get; }

        /// <summary>Text for saving files.</summary>
        string Save { get; }

        /// <summary>Text for deleting files or folders.</summary>
        string Delete { get; }

        /// <summary>Text for creating a new folder.</summary>
        string CreateNewFolder { get; }

        /// <summary>Placeholder text for a new folder name.</summary>
        string NewFolderPlaceHolder { get; }

        /// <summary>Text for creating a new file.</summary>
        string CreateNewFile { get; }

        /// <summary>Placeholder text for a new file name.</summary>
        string NewFilePlaceHolder { get; }

        /// <summary>Text for viewing files.</summary>
        string View { get; }

        /// <summary>Text for downloading files.</summary>
        string Download { get; }

        /// <summary>Text for searching files or folders.</summary>
        string Search { get; }

        /// <summary>Text for zipping files or folders.</summary>
        string Zip { get; }

        /// <summary>Text for unzipping files or folders.</summary>
        string Unzip { get; }

        /// <summary>Text for retrieving the contents of a folder.</summary>
        string GetFolderContent { get; }

        /// <summary>Text for retrieving the contents of a file.</summary>
        string GetFileContent { get; }

        /// <summary>Text for uploading files.</summary>
        string Upload { get; }

        /// <summary>Text for toggling between different views.</summary>
        string ToggleView { get; }

        /// <summary>Text for reloading the file manager view.</summary>
        string Reload { get; }

        /// <summary>Text for breadcrumb navigation.</summary>
        string Breadcrumb { get; }

        /// <summary>Text for displaying the folder tree.</summary>
        string FoldersTree { get; }

        /// <summary>Text for the menu bar.</summary>
        string MenuBar { get; }

        /// <summary>Text for the context menu.</summary>
        string ContextMenu { get; }

        /// <summary>Text for the file preview area.</summary>
        string FilePreview { get; }

        /// <summary>Text for the creation date label.</summary>
        string CreateDate { get; }

        /// <summary>Text for the modification date label.</summary>
        string ModifiedDate { get; }

        /// <summary>Text for the file name label.</summary>
        string FileName { get; }

        /// <summary>Text for the folder name label.</summary>
        string FolderName { get; }

        /// <summary>Text for the file or folder size label.</summary>
        string Size { get; }

        /// <summary>Text for going back to the previous directory.</summary>
        string Back { get; }

        /// <summary>Text for navigating up one level in the directory.</summary>
        string Up { get; }

        /// <summary>Text for closing a dialog or window.</summary>
        string Close { get; }

        /// <summary>Text for is encryption status</summary>
        string EncryptionStatus { get; }

        /// <summary>Text for is encrypted</summary>
        string IsEncrypted { get; }

        /// <summary>Text for is not encrypted</summary>
        string NotEncrypted { get; }

        /// <summary>Text for encryption explanation</summary>
        string EncryptedFileExplanation { get; }

        // Notifications and Messages

        /// <summary>Message shown when no items are selected.</summary>
        string NoItemsSelectedMessage { get; }

        /// <summary>Message prompting for a new folder name.</summary>
        string EnterNewFolderNameMessage { get; }

        /// <summary>Message prompting for a new file name.</summary>
        string EnterNewFileNameMessage { get; }

        /// <summary>Confirmation message for deleting items.</summary>
        string DeleteConfirmationMessage { get; }

        /// <summary>Message prompting for a new name during renaming.</summary>
        string RenameMessage { get; }

        /// <summary>Message shown when an item already exists and may be overwritten.</summary>
        string ItemAlreadyExistMessage { get; }

        /// <summary>Message prompting for a zip file name.</summary>
        string ZipFileNameMessage { get; }

        /// <summary>Confirmation message for overwriting existing items.</summary>
        string OverrideConfirmationMessage { get; }

        /// <summary>
        /// Message shown when an unknown command is entered.
        /// </summary>
        string UnknownCommandErrorMessage { get; }

        /// <summary>
        /// Message shown when an invalid root path is entered.
        /// </summary>
        string InvalidRootPathErrorMessage { get; }

        /// <summary>
        /// Message shown when not enough space is available.
        /// </summary>
        string NotEnoughSpaceErrorMessage { get; }

        /// <summary>
        /// Message shown when a file is too big to be uploaded.
        /// </summary>
        string TooBigErrorMessage { get; }

        /// <summary>
        /// Message shown when an action is disabled.
        /// </summary>
        string ActionDisabledErrorMessage { get; }

        /// <summary>
        /// Message shown when a file or folder is not found.
        /// </summary>
        string NotFoundErrorMessage { get; }

        /// <summary>
        /// Message shown when a file is not editable.
        /// </summary>
        string IsNotEditableFileErrorMessage { get; }

        /// <summary>
        /// Message shown when files are not accepted.
        /// </summary>
        string FilesNotAcceptedErrorMessage { get; }

        /// <summary>
        /// Message shown when a file already exists.
        /// </summary>
        string FileAlreadyExistsErrorMessage { get; }



        // Dropzone-related messages

        /// <summary>Default message for the Dropzone area.</summary>
        string DictDefaultMessage { get; }

        /// <summary>Message shown when the browser does not support file uploads.</summary>
        string DictFallbackMessage { get; }

        /// <summary>Fallback text for browsers that do not support Dropzone functionality.</summary>
        string DictFallbackText { get; }

        /// <summary>Message shown when a file is too large to upload.</summary>
        string DictFileTooBig { get; }

        /// <summary>Message shown when the file type is invalid.</summary>
        string DictInvalidFileType { get; }

        /// <summary>Message shown when the server responds with an error code.</summary>
        string DictResponseError { get; }

        /// <summary>Text for canceling an upload.</summary>
        string DictCancelUpload { get; }

        /// <summary>Confirmation message for canceling an upload.</summary>
        string DictCancelUploadConfirmation { get; }

        /// <summary>Text for removing a file from the upload queue.</summary>
        string DictRemoveFile { get; }

        /// <summary>Message shown when the maximum file limit is exceeded.</summary>
        string DictMaxFilesExceeded { get; }
    }
}