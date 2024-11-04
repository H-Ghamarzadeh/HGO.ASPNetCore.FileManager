namespace HGO.ASPNetCore.FileManager.DTOs;

internal class ZipCommandParameters
{
    public string Path { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new List<string>();
    public string FileName { get; set; } = string.Empty;
}