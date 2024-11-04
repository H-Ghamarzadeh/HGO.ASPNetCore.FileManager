using HGO.ASPNetCore.FileManager.Models.LanguageModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.ViewModels
{
    public class EditViewModel
    {
        //result.TempData["Id"] = id;
        //result.TempData["FileFullPath"] = filePath;
        //result.TempData["FileName"] = Path.GetFileName(physicalPath);
        //result.TempData["FileData"] = File.ReadAllText(physicalPath);

        public required string Id { get; set; }
        public required string FileFullPath { get; set; }
        public required string FileName { get; set; }
        public required string FileData { get; set; }
        public required ILanguage Language { get; set; }
    }
}
