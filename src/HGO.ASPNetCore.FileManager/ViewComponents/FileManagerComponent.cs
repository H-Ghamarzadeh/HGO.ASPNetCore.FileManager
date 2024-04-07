using HGO.ASPNetCore.FileManager.ViewComponentsModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HGO.ASPNetCore.FileManager.ViewComponents
{
    public class FileManagerComponent: ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string RootPathSessionKey = "HGO-FM-RootFolder";
        public static Dictionary<string, FileManagerConfig> ConfigStorage = new Dictionary<string, FileManagerConfig>();

        public FileManagerComponent(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

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
            _httpContextAccessor.HttpContext.Session.SetString(RootPathSessionKey + model.Id, model.RootFolder);

            //return component view
            return View(model);
        }
    }
}
