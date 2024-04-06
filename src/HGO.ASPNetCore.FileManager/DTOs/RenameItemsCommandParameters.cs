namespace HGO.ASPNetCore.FileManager.DTOs;

internal class RenameItemsCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
    public string NewName { get; set; }
}