using HGO.ASPNetCore.FileManager.Models.LanguageModels;
using HGO.ASPNetCore.FileManager.Models.LanguageModels.BuiltIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HGO.ASPNetCore.FileManager.ViewModels
{
    public class EditViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FileFullPath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileData { get; set; } = string.Empty;    
        public ILanguage Language { get; set; } = new EnglishLanguage();
    }
}
