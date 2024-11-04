using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HGO.ASPNetCore.FileManager.Models.LanguageModels
{
    /// <summary>
    /// Base class for all languages.
    /// </summary>
    public abstract class LanguageBase : ILanguage
    {
        /// <summary>Text for browsing files.</summary>
        public string Browse { get; } = null!;

        /// <summary>Text for copying files or folders.</summary>
        public string Copy { get; } = null!;

        /// <summary>Text for cutting files or folders.</summary>
        public string Cut { get; } = null!;

        /// <summary>Text for pasting files or folders.</summary>
        public string Paste { get; } = null!;

        /// <summary>Text for renaming files or folders.</summary>
        public string Rename { get; } = null!;

        /// <summary>Text for editing files.</summary>
        public string Edit { get; } = null!;

        /// <summary>Text for saving files.</summary>
        public string Save { get; } = null!;

        /// <summary>Text for deleting files or folders.</summary>
        public string Delete { get; } = null!;

        /// <summary>Text for creating a new folder.</summary>
        public string CreateNewFolder { get; } = null!;

        /// <summary>Placeholder text for a new folder name.</summary>
        public string NewFolderPlaceHolder { get; } = null!;

        /// <summary>Text for creating a new file.</summary>
        public string CreateNewFile { get; } = null!;

        /// <summary>Placeholder text for a new file name.</summary>
        public string NewFilePlaceHolder { get; } = null!;

        /// <summary>Text for viewing files.</summary>
        public string View { get; } = null!;

        /// <summary>Text for downloading files.</summary>
        public string Download { get; } = null!;

        /// <summary>Text for searching files or folders.</summary>
        public string Search { get; } = null!;

        /// <summary>Text for zipping files or folders.</summary>
        public string Zip { get; } = null!;

        /// <summary>Text for unzipping files or folders.</summary>
        public string Unzip { get; } = null!;

        /// <summary>Text for retrieving the contents of a folder.</summary>
        public string GetFolderContent { get; } = null!;

        /// <summary>Text for retrieving the contents of a file.</summary>
        public string GetFileContent { get; } = null!;

        /// <summary>Text for uploading files.</summary>
        public string Upload { get; } = null!;

        /// <summary>Text for toggling between different views.</summary>
        public string ToggleView { get; } = null!;

        /// <summary>Text for reloading the file manager view.</summary>
        public string Reload { get; } = null!;

        /// <summary>Text for breadcrumb navigation.</summary>
        public string Breadcrumb { get; } = null!;

        /// <summary>Text for displaying the folder tree.</summary>
        public string FoldersTree { get; } = null!;

        /// <summary>Text for the menu bar.</summary>
        public string MenuBar { get; } = null!;

        /// <summary>Text for the context menu.</summary>
        public string ContextMenu { get; } = null!;

        /// <summary>Text for the file preview area.</summary>
        public string FilePreview { get; } = null!;

        /// <summary>Text for the creation date label.</summary>
        public string CreateDate { get; } = null!;

        /// <summary>Text for the modification date label.</summary>
        public string ModifiedDate { get; } = null!;

        /// <summary>Text for the file name label.</summary>
        public string FileName { get; } = null!;

        /// <summary>Text for the folder name label.</summary>
        public string FolderName { get; } = null!;

        /// <summary>Text for the file or folder size label.</summary>
        public string Size { get; } = null!;

        /// <summary>Text for going back to the previous directory.</summary>
        public string Back { get; } = null!;

        /// <summary>Text for navigating up one level in the directory.</summary>
        public string Up { get; } = null!;

        /// <summary>Text for closing a dialog or window.</summary>
        public string Close { get; } = null!;

        // Notifications and Messages

        /// <summary>Message shown when no items are selected.</summary>
        public string NoItemsSelectedMessage { get; } = null!;

        /// <summary>Message prompting for a new folder name.</summary>
        public string EnterNewFolderNameMessage { get; } = null!;

        /// <summary>Message prompting for a new file name.</summary>
        public string EnterNewFileNameMessage { get; } = null!;

        /// <summary>Confirmation message for deleting items.</summary>
        public string DeleteConfirmationMessage { get; } = null!;

        /// <summary>Message prompting for a new name during renaming.</summary>
        public string RenameMessage { get; } = null!;

        /// <summary>Message shown when an item already exists and may be overwritten.</summary>
        public string ItemAlreadyExistMessage { get; } = null!;

        /// <summary>Message prompting for a zip file name.</summary>
        public string ZipFileNameMessage { get; } = null!;

        /// <summary>Confirmation message for overwriting existing items.</summary>
        public string OverrideConfirmationMessage { get; } = null!;

        // Dropzone-related messages

        /// <summary>Default message for the Dropzone area.</summary>
        public string DictDefaultMessage { get; } = null!;

        /// <summary>Message shown when the browser does not support file uploads.</summary>
        public string DictFallbackMessage { get; } = null!;

        /// <summary>Fallback text for browsers that do not support Dropzone functionality.</summary>
        public string DictFallbackText { get; } = null!;

        /// <summary>Message shown when a file is too large to upload.</summary>
        public string DictFileTooBig { get; } = null!;

        /// <summary>Message shown when the file type is invalid.</summary>
        public string DictInvalidFileType { get; } = null!;

        /// <summary>Message shown when the server responds with an error code.</summary>
        public string DictResponseError { get; } = null!;

        /// <summary>Text for canceling an upload.</summary>
        public string DictCancelUpload { get; } = null!;

        /// <summary>Confirmation message for canceling an upload.</summary>
        public string DictCancelUploadConfirmation { get; } = null!;

        /// <summary>Text for removing a file from the upload queue.</summary>
        public string DictRemoveFile { get; } = null!;

        /// <summary>Message shown when the maximum file limit is exceeded.</summary>
        public string DictMaxFilesExceeded { get; } = null!;

        /// <summary>
        /// if needed...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Encode(string value) => HttpUtility.HtmlEncode(value);

        /// <summary>
        /// if needed...
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Decode(string value) => HttpUtility.HtmlDecode(value);


    }
}
