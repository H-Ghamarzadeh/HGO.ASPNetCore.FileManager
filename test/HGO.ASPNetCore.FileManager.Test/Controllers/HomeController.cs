using HGO.ASPNetCore.FileManager.Test.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using HGO.ASPNetCore.FileManager.CommandsProcessor;
using HGO.ASPNetCore.FileManager.Enums;
using HGO.ASPNetCore.FileManager.ViewComponentsModel.LanguageModels.BuiltIn;
using HGO.ASPNetCore.FileManager.ViewComponentsModel;
using System.Reflection;
using HGO.ASPNetCore.FileManager.ViewComponentsModel.LanguageModels;

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

        [HttpGet]
        public IActionResult Index()
        {
            var config = new FileManagerConfig()
            {
                Language = new EnglishLanguage(),
                UseEncryption = true,
                EncryptionKey = "1234567890123456",
            };

            return View(config);
        }

        [HttpPost]
        public IActionResult Index(FileManagerConfig config, string Language)
        {
            // Check if the language type string is provided and not null
            if (!string.IsNullOrEmpty(Language))
            {
                // Attempt to get the type directly
                var languageType = Type.GetType(Language);

                // If null, try searching across all loaded assemblies
                if (languageType == null)
                {
                    // Get all assemblies currently loaded in the application domain
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (var assembly in assemblies)
                    {
                        languageType = assembly.GetType(Language);
                        if (languageType != null && typeof(ILanguage).IsAssignableFrom(languageType))
                        {
                            break;
                        }
                    }
                }

                // Check if the languageType was successfully found and create an instance if so
                if (languageType != null && typeof(ILanguage).IsAssignableFrom(languageType))
                {
                    config.Language = (ILanguage?)Activator.CreateInstance(languageType) ?? new EnglishLanguage();
                }
                else
                {
                    // Log or handle the failure case
                    _logger.LogWarning("Language type '{Language}' could not be found or is not assignable to ILanguage.", Language);
                }
            }


            return View(config);
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
