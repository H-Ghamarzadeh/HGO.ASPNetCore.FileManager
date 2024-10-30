using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Models.Langugage
{
    public class CustomLanguage : ILanguage
    {
        public string Browse { get; set; } = "*Not Defined*";

        public string Copy { get; set; } = "*Not Defined*";

        public string Cut { get; set; } = "*Not Defined*";

        public string Paste { get; set; } = "*Not Defined*";

        public string Rename { get; set; } = "*Not Defined*";

        public string Edit { get; set; } = "*Not Defined*";

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

        public string NoItemsSelectedMessage { get; set; } = "*Not Defined*";
    }
}
