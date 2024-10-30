using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Models.Langugage
{
    public interface ILanguage
    {
        string Browse { get; }
        string Copy { get; }
        string Cut { get; }
        string Paste { get; }
        string Rename { get; }
        string Edit { get; }
        string Delete { get; }
        string CreateNewFolder { get; }
        string NewFolderPlaceHolder { get; }
        string CreateNewFile { get; }
        string NewFilePlaceHolder { get; }
        string View { get; }
        string Download { get; }
        string Search { get; }
        string Zip { get; }
        string Unzip { get; }
        string GetFolderContent { get; }
        string GetFileContent { get; }
        string Upload { get; }
        string ToggleView { get; }
        string Reload { get; }
        string Breadcrumb { get; }
        string FoldersTree { get; }
        string MenuBar { get; }
        string ContextMenu { get; }
        string FilePreview { get; }

        // Notifications And Messages
        string NoItemsSelectedMessage { get; }
    }
}
