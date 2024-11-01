using HGO.ASPNetCore.FileManager.Models.LangugageModels;
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

        public string Id { get; set; }
        public string FileFullPath { get; set; }
        public string FileName { get; set; }
        public string FileData { get; set; }
        public ILanguage Language { get; set; }
    }
}
