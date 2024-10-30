using HGO.ASPNetCore.FileManager.Test.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HGO.ASPNetCore.FileManager.CommandsProcessor;
using HGO.ASPNetCore.FileManager.Enums;

namespace HGO.ASPNetCore.FileManager.Test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IFileManagerCommandsProcessor _processor;

        public HomeController(ILogger<HomeController> logger, IFileManagerCommandsProcessor processor)
        {
            _logger = logger;
            _processor = processor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost, HttpGet]
        public async Task<IActionResult> HgoApi(string id, Command command, string parameters, IFormFile file)
        {
            return await _processor.ProcessCommandAsync(id, command, parameters, file);
        }
    }
}
