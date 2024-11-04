namespace HGO.ASPNetCore.FileManager.DTOs;

internal class ZipCommandParameters
{
    public string Path { get; set; } = null!;
    public List<string> Items { get; set; } = null!;
    public string FileName { get; set; } = null!;
}