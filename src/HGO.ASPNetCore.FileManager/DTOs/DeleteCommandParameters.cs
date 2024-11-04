namespace HGO.ASPNetCore.FileManager.DTOs;

internal class DeleteCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
}