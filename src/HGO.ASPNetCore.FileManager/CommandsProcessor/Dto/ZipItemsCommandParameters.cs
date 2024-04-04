namespace HGO.ASPNetCore.FileManager.CommandsProcessor.Dto;

internal class ZipItemsCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
    public string FileName { get; set; }
}