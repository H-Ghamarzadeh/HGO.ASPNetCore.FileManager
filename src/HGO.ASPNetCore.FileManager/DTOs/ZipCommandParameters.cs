namespace HGO.ASPNetCore.FileManager.DTOs;

internal class ZipCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
    public string FileName { get; set; }
}