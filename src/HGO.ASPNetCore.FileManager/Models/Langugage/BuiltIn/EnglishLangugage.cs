using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Models.Langugage.BuiltIn
{
    public class EnglishLanguage : ILanguageModel
    {
        public string Browse { get; set; } = "Browse";
        public string Copy { get; set; } = "Copy";
        public string Cut { get; set; } = "Cut";
        public string Paste { get; set; } = "Paste";
        public string Rename { get; set; } = "Rename";
        public string Edit { get; set; } = "Edit";
        public string Delete { get; set; } = "Delete";
        public string CreateNewFolder { get; set; } = "Create New Folder";
        public string CreateNewFile { get; set; } = "Create New File";
        public string View { get; set; } = "View";
        public string Download { get; set; } = "Download";
        public string Search { get; set; } = "Search";
        public string Zip { get; set; } = "Zip";
        public string Unzip { get; set; } = "Unzip";
        public string GetFolderContent { get; set; } = "Get Folder Content";
        public string GetFileContent { get; set; } = "Get File Content";
        public string Upload { get; set; } = "Upload";
        public string ToggleView { get; set; } = "Toggle View";
        public string Reload { get; set; } = "Reload";
        public string Breadcrumb { get; set; } = "Breadcrumb";
        public string FoldersTree { get; set; } = "Folders Tree";
        public string MenuBar { get; set; } = "Menu Bar";
        public string ContextMenu { get; set; } = "Context Menu";
        public string FilePreview { get; set; } = "File Preview";
        public string NewFolderPlaceHolder { get; set; } = "New Folder";
        public string NewFilePlaceHolder { get; set; } = "New Text.txt";
        public string NoItemsSelectedMessage { get; set; } = "Please select your desired item(s).";
    }

}
