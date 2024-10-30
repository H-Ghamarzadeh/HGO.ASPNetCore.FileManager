using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using HGO.ASPNetCore.FileManager.CommandsProcessor;
using HGO.ASPNetCore.FileManager.ViewComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;

namespace HGO.ASPNetCore.FileManager
{
    public static class Extensions
    {
        public static void AddHgoFileManager(this IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation(c=> c.FileProviders.Add(new EmbeddedFileProvider(typeof(FileManagerComponent)
                .GetTypeInfo().Assembly, "HGO.ASPNetCore.FileManager")));
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(IFileManagerCommandsProcessor), typeof(FileManagerCommandsProcessor));
        }

        public static void UseHgoFileManager(this IApplicationBuilder builder)
        {
            var embeddedProvider = new EmbeddedFileProvider(typeof(FileManagerComponent)
                .GetTypeInfo().Assembly, "HGO.ASPNetCore.FileManager.hgofilemanager");

            builder.UseSession();
            builder.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = embeddedProvider,
                RequestPath = new PathString("/hgofilemanager")
            });
        }

        public static HtmlString RenderHgoFileManagerJavaScripts(this RazorPageBase razorPage, string[]? exclude = null)
        {
            var excludelist = new List<string>();
            if (exclude is { Length: > 0 })
            {
                excludelist.AddRange(exclude);
            }

            var scripts = "";
            if (!excludelist.Any(p => p.ToLower() == "js.cookie"))
            {
                scripts +=
                    "<script src='hgofilemanager/js.cookie/js.cookie.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "fontawesome"))
            {
                scripts +=
                    "<script src='hgofilemanager/fontawesome/js/fontawesome.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "lozad.js"))
            {
                scripts +=
                    "<script src='hgofilemanager/lozad.js/lozad.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "jsTree"))
            {
                scripts +=
                    "<script src='hgofilemanager/jsTree/jstree.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "viselect"))
            {
                scripts +=
                    "<script src='hgofilemanager/viselect/viselect.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "split.js"))
            {
                scripts +=
                    "<script src='hgofilemanager/split.js/split.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "dropzone"))
            {
                scripts +=
                    "<script src='hgofilemanager/dropzone/dropzone.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "context-js"))
            {
                scripts +=
                    "<script src='hgofilemanager/context-js/context/context.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "toastify"))
            {
                scripts +=
                    "<script src='hgofilemanager/toastify/toastify.js'></script>\r\n";
            }

            //scripts += "<script src='hgofilemanager/HgoFileManager.js'></script>\r\n";
            return new HtmlString(scripts);
        }

        public static HtmlString RenderHgoFileManagerCss(this RazorPageBase razorPage, bool darkMode = false, string[]? exclude = null)
        {
            var excludelist = new List<string>();
            if (exclude is { Length: > 0 })
            {
                excludelist.AddRange(exclude);
            }

            var styles = "";
            if (!excludelist.Any(p => p.ToLower() == "jsTree"))
            {
                styles +=
                    "<link rel='stylesheet' href='hgofilemanager/jsTree/themes/default/style.min.css' type='text/css' />\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "dropzone"))
            {
                styles +=
                    "<link rel='stylesheet' href='hgofilemanager/dropzone/dropzone.css' type='text/css' />\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "fontawesome"))
            {
                styles +=
                    "<link rel='stylesheet' href='hgofilemanager/fontawesome/css/all.min.css' type='text/css' />\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "context-js"))
            {
                if (darkMode)
                {
                    styles +=
                        "<link rel='stylesheet' href='hgofilemanager/context-js/context/skins/kali_dark.css' type='text/css' />\r\n";
                }
                else
                {
                    styles +=
                        "<link rel='stylesheet' href='hgofilemanager/context-js/context/skins/kali_light.css' type='text/css' />\r\n";
                }
            }
            if (!excludelist.Any(p => p.ToLower() == "toastify"))
            {
                styles +=
                    "<link rel='stylesheet' href='hgofilemanager/toastify/toastify.css' type='text/css' />\r\n";
            }

            styles += "<link rel='stylesheet' href='hgofilemanager/HgoFileManager.css' type='text/css' />\r\n";

            if (darkMode)
            {
                styles += "<link rel='stylesheet' href='hgofilemanager/HgoFileManager-dark.css' type='text/css' />\r\n";
                razorPage.ViewContext.HttpContext.Session.SetString("HgoFileManagerTheme", "Dark");
            }

            return new HtmlString(styles);
        }
    }
}
