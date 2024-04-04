using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HGO.ASPNetCore.FileManager.CommandsProcessor
{
    public interface IFileManagerCommandsProcessor
    {
        Task<IActionResult> ProcessCommandAsync(string id, string command, string parameters, IFormFile file);
    }
}
