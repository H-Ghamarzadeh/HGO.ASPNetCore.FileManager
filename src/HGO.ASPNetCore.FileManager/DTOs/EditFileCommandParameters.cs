namespace HGO.ASPNetCore.FileManager.DTOs;

internal class EditFileCommandParameters
{
    public string FilePath { get; set; } = null!;
    public string Data { get; set; } = null!;
}