namespace HGO.ASPNetCore.FileManager.DTOs;

internal class RenameCommandParameters
{
    public string Path { get; set; } = null!;
    public List<string> Items { get; set; } = null!;
    public string NewName { get; set; } = null!;
}