namespace HGO.ASPNetCore.FileManager.DTOs;

internal class UnZipCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
}