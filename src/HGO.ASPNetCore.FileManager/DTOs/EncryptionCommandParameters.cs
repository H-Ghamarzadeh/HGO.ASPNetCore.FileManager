namespace HGO.ASPNetCore.FileManager.DTOs;

internal class EncryptionCommandParameters
{
    public string Path { get; set; } = string.Empty;
    public List<string> Items { get; set; } = new List<string>();
}