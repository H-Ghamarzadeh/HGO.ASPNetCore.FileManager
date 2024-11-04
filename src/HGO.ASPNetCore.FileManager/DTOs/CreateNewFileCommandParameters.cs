namespace HGO.ASPNetCore.FileManager.DTOs;

internal class CreateNewFileCommandParameters
{
    public string Path { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}