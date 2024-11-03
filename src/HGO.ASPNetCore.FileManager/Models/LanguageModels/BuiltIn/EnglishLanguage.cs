using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Models.LanguageModels.BuiltIn
{
    public class EnglishLanguage : LanguageBase, ILanguage
    {
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
        public string Back { get; } = "Back";
        public string Up { get; } = "Up";
        public string Close { get; } = "Close";
        public string EnterNewFolderNameMessage { get; } = "Please enter folder name:";
        public string EnterNewFileNameMessage { get; } = "Please enter your desired file name:";
        public string DeleteConfirmationMessage { get; } = "Are you sure you want to delete selected items?";
        public string RenameMessage { get; } = "Please enter new name:";
        public string ItemAlreadyExistMessage { get; } = "already exist, do you want to overwrite?";
        public string ZipFileNameMessage { get; } = "Please enter Zip file name:";
        public string OverrideConfirmationMessage { get; } = "Some item(s) already exist, do you want to overwrite?";
    }

}
