namespace HGO.ASPNetCore.FileManager.DTOs;

internal class CreateNewFolderCommandParameters
{
    public string Path { get; set; } = null!;
    public string FolderName { get; set; } = null!;
}