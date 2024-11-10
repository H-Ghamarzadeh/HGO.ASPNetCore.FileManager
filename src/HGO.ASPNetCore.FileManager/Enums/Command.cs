using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.Enums
{
    /// <summary>
    /// Enum representing different file-related functions.
    /// </summary>

    public enum Command
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        /// <summary>
        /// in case of unknown command sent to api somehow (it has no effect on functionality)
        /// </summary>
        Unknown,
        Search,
        CreateNewFolder,
        CreateNewFile,
        Delete,
        Rename,
        Zip,
        Unzip,
        Copy,
        Cut,
        EditFile,
        Download,
        GetFolderContent,
        GetFileContent,
        Upload,
        ToggleView,
        Browse,
        Reload,
        Breadcrumb,
        FoldersTree,
        MenuBar,
        ContextMenu,
        FilePreview,
        View,
        Encrypt,
        Decrypt
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
