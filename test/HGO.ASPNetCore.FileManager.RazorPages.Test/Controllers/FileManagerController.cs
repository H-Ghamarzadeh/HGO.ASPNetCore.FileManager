using HGO.ASPNetCore.FileManager.CommandsProcessor;
using Microsoft.AspNetCore.Mvc;

namespace HGO.ASPNetCore.FileManager.RazorPages.Test.Controllers
{
    public class FileManagerController : Controller
    {
        private readonly IFileManagerCommandsProcessor _processor;

        public FileManagerController(IFileManagerCommandsProcessor processor)
        {
            _processor = processor;
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> Index(string id, string command, string parameters, IFormFile file)
        {
            return await _processor.ProcessCommandAsync(id, command, parameters, file);
        }
    }
}
