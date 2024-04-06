namespace HGO.ASPNetCore.FileManager.DTOs;

internal class ExtractItemsCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
}