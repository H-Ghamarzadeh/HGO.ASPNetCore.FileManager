using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Models.Langugage
{
    public interface ILanguageModel
    {
        public string Browse { get; set; }
        public string Copy { get; set; }
        public string Cut { get; set; }
        public string Paste { get; set; }
        public string Rename { get; set; }
        public string Edit { get; set; }
        public string Delete { get; set; }
        public string CreateNewFolder { get; set; }
        public string NewFolderPlaceHolder { get; set; }
        public string CreateNewFile { get; set; }
        public string NewFilePlaceHolder { get; set; }
        public string View { get; set; }
        public string Download { get; set; }
        public string Search { get; set; }
        public string Zip { get; set; }
        public string Unzip { get; set; }
        public string GetFolderContent { get; set; }
        public string GetFileContent { get; set; }
        public string Upload { get; set; }
        public string ToggleView { get; set; }
        public string Reload { get; set; }
        public string Breadcrumb { get; set; }
        public string FoldersTree { get; set; }
        public string MenuBar { get; set; }
        public string ContextMenu { get; set; }
        public string FilePreview { get; set; }

        //Notifications And Messages
        public string NoItemsSelectedMessage { get; set; }
    }
}
