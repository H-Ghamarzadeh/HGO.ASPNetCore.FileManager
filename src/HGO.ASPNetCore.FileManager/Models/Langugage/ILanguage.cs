using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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
        string CreateDate { get; }
        string ModifiedDate { get; }
        string Name { get; }
        string Size { get; }
        string Back { get; }
        string Up { get; }
        string Close { get; }


        // Notifications And Messages
        string NoItemsSelectedMessage { get; }
        string EnterNewFolderNameMessage { get; }
        string EnterNewFileNameMessage { get; }
        string DeleteConfirmationMessage { get; }
        string RenameMessage { get; }
        string ItemAlreadyExistMessage { get; }
        string ZipFileNameMessage { get; }
        string OverrideConfirmationMessage { get; }
    }
}
