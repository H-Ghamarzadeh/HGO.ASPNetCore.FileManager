namespace HGO.ASPNetCore.FileManager.DTOs;

internal class CreateNewFileCommandParameters
{
    public string Path { get; set; } = null!;
    public string FileName { get; set; } = null!;
}