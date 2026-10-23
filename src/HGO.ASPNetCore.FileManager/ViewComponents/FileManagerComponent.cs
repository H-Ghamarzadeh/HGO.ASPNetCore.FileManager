using HGO.ASPNetCore.FileManager.ViewComponentsModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HGO.ASPNetCore.FileManager.ViewComponents
{
    /// <summary>Renders and initializes an instance of the file-manager UI.</summary>
    public class FileManagerComponent: ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string RootPathSessionKey = "HGO-FM-RootFolder";
        /// <summary>Gets the in-memory configuration for active file-manager instances.</summary>
        public static Dictionary<string, FileManagerConfig> ConfigStorage = new Dictionary<string, FileManagerConfig>();

        /// <summary>Initializes the file-manager component.</summary>
        /// <param name="httpContextAccessor">Provides access to the current HTTP context.</param>
        public FileManagerComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>Initializes the requested instance and renders its view.</summary>
        /// <param name="model">The instance configuration and root-folder information.</param>
        /// <returns>The view component result.</returns>
        public IViewComponentResult Invoke(FileManagerModel model)
        {
            //Check if ID isn't null
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                throw new ArgumentNullException(model.Id);
            }

            model.Id = "hgo_fm_"+ model.Id;
            ConfigStorage[model.Id] = model.Config;

            //Check if directory exist
            if (!Directory.Exists(model.RootFolder))
            {
                Directory.CreateDirectory(model.RootFolder);
            }

            //Add root path to session
            _httpContextAccessor.HttpContext!.Session.SetString(RootPathSessionKey + model.Id, model.RootFolder);

            //return component view
            return View(model);
        }
    }
}
