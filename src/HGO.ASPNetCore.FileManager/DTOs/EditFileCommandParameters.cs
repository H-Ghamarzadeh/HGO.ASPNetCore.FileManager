namespace HGO.ASPNetCore.FileManager.DTOs;

internal class EditFileCommandParameters
{
    public string FilePath { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}