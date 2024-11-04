namespace HGO.ASPNetCore.FileManager.DTOs;

internal class RenameCommandParameters
{
    public string Path { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new List<string>();
    public string NewName { get; set; } = string.Empty;
}