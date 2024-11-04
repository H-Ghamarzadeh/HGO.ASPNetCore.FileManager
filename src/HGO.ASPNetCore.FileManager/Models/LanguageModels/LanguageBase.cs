﻿using System;
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
        public string Browse { get; }

        /// <summary>Text for copying files or folders.</summary>
        public string Copy { get; }

        /// <summary>Text for cutting files or folders.</summary>
        public string Cut { get; }

        /// <summary>Text for pasting files or folders.</summary>
        public string Paste { get; }

        /// <summary>Text for renaming files or folders.</summary>
        public string Rename { get; }

        /// <summary>Text for editing files.</summary>
        public string Edit { get; }

        /// <summary>Text for saving files.</summary>
        public string Save { get; }

        /// <summary>Text for deleting files or folders.</summary>
        public string Delete { get; }

        /// <summary>Text for creating a new folder.</summary>
        public string CreateNewFolder { get; }

        /// <summary>Placeholder text for a new folder name.</summary>
        public string NewFolderPlaceHolder { get; }

        /// <summary>Text for creating a new file.</summary>
        public string CreateNewFile { get; }

        /// <summary>Placeholder text for a new file name.</summary>
        public string NewFilePlaceHolder { get; }

        /// <summary>Text for viewing files.</summary>
        public string View { get; }

        /// <summary>Text for downloading files.</summary>
        public string Download { get; }

        /// <summary>Text for searching files or folders.</summary>
        public string Search { get; }

        /// <summary>Text for zipping files or folders.</summary>
        public string Zip { get; }

        /// <summary>Text for unzipping files or folders.</summary>
        public string Unzip { get; }

        /// <summary>Text for retrieving the contents of a folder.</summary>
        public string GetFolderContent { get; }

        /// <summary>Text for retrieving the contents of a file.</summary>
        public string GetFileContent { get; }

        /// <summary>Text for uploading files.</summary>
        public string Upload { get; }

        /// <summary>Text for toggling between different views.</summary>
        public string ToggleView { get; }

        /// <summary>Text for reloading the file manager view.</summary>
        public string Reload { get; }

        /// <summary>Text for breadcrumb navigation.</summary>
        public string Breadcrumb { get; }

        /// <summary>Text for displaying the folder tree.</summary>
        public string FoldersTree { get; }

        /// <summary>Text for the menu bar.</summary>
        public string MenuBar { get; }

        /// <summary>Text for the context menu.</summary>
        public string ContextMenu { get; }

        /// <summary>Text for the file preview area.</summary>
        public string FilePreview { get; }

        /// <summary>Text for the creation date label.</summary>
        public string CreateDate { get; }

        /// <summary>Text for the modification date label.</summary>
        public string ModifiedDate { get; }

        /// <summary>Text for the file name label.</summary>
        public string FileName { get; }

        /// <summary>Text for the folder name label.</summary>
        public string FolderName { get; }

        /// <summary>Text for the file or folder size label.</summary>
        public string Size { get; }

        /// <summary>Text for going back to the previous directory.</summary>
        public string Back { get; }

        /// <summary>Text for navigating up one level in the directory.</summary>
        public string Up { get; }

        /// <summary>Text for closing a dialog or window.</summary>
        public string Close { get; }

        // Notifications and Messages

        /// <summary>Message shown when no items are selected.</summary>
        public string NoItemsSelectedMessage { get; }

        /// <summary>Message prompting for a new folder name.</summary>
        public string EnterNewFolderNameMessage { get; }

        /// <summary>Message prompting for a new file name.</summary>
        public string EnterNewFileNameMessage { get; }

        /// <summary>Confirmation message for deleting items.</summary>
        public string DeleteConfirmationMessage { get; }

        /// <summary>Message prompting for a new name during renaming.</summary>
        public string RenameMessage { get; }

        /// <summary>Message shown when an item already exists and may be overwritten.</summary>
        public string ItemAlreadyExistMessage { get; }

        /// <summary>Message prompting for a zip file name.</summary>
        public string ZipFileNameMessage { get; }

        /// <summary>Confirmation message for overwriting existing items.</summary>
        public string OverrideConfirmationMessage { get; }

        /// <summary>
        /// Message shown when an unknown command is entered.
        /// </summary>
        public string UnknownCommandErrorMessage { get; }

        /// <summary>
        /// Message shown when the root path is invalid.
        /// </summary>
        public string InvalidRootPathErrorMessage { get; }

        /// <summary>
        /// Message shown when there is not enough space.
        /// </summary>
        public string NotEnoughSpaceErrorMessage { get; }

        /// <summary>
        /// Message shown when a file is too big to perform specified action.
        /// </summary>
        /// 
        public string TooBigErrorMessage { get; }

        /// <summary>
        /// Message shown when an action is disabled.
        /// </summary>
        public string ActionDisabledErrorMessage { get; }

        /// <summary>
        /// Message shown when a file is not found.
        /// </summary>
        public string NotFoundErrorMessage { get; }

        /// <summary>
        /// Message shown when a file is not editable.
        /// </summary>
        public string IsNotEditableFileErrorMessage { get; }

        /// <summary>
        /// Message shown when a file is not accepted.
        /// </summary>
        public string FilesNotAcceptedErrorMessage { get; }

        /// <summary>
        /// Message shown when a file already exists.
        /// </summary>
        public string FileAlreadyExistsErrorMessage { get; }

        // Dropzone-related messages

        /// <summary>Default message for the Dropzone area.</summary>
        public string DictDefaultMessage { get; }

        /// <summary>Message shown when the browser does not support file uploads.</summary>
        public string DictFallbackMessage { get; }

        /// <summary>Fallback text for browsers that do not support Dropzone functionality.</summary>
        public string DictFallbackText { get; }

        /// <summary>Message shown when a file is too large to upload.</summary>
        public string DictFileTooBig { get; }

        /// <summary>Message shown when the file type is invalid.</summary>
        public string DictInvalidFileType { get; }

        /// <summary>Message shown when the server responds with an error code.</summary>
        public string DictResponseError { get; }

        /// <summary>Text for canceling an upload.</summary>
        public string DictCancelUpload { get; }

        /// <summary>Confirmation message for canceling an upload.</summary>
        public string DictCancelUploadConfirmation { get; }

        /// <summary>Text for removing a file from the upload queue.</summary>
        public string DictRemoveFile { get; }

        /// <summary>Message shown when the maximum file limit is exceeded.</summary>
        public string DictMaxFilesExceeded { get; }

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
