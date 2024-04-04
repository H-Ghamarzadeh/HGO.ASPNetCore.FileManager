namespace HGO.ASPNetCore.FileManager.CommandsProcessor.Dto;

internal class DeleteItemsCommandParameters
{
    public string Path { get; set; }
    public List<string> Items { get; set; }
}