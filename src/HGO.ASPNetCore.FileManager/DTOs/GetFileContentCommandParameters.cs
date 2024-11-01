using HGO.ASPNetCore.FileManager.Models.LangugageModels;

namespace HGO.ASPNetCore.FileManager.DTOs;

internal class GetFileContentCommandParameters
{
    public string FilePath { get; set; }
    public string LangugageJson { get; set; }
}