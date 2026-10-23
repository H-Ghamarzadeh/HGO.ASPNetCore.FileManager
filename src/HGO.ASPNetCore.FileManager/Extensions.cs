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
    /// <summary>Provides registration, middleware, and rendering helpers for the file manager.</summary>
    public static class Extensions
    {
        /// <summary>Registers the file-manager services and its embedded views.</summary>
        /// <param name="services">The application's service collection.</param>
        public static void AddHgoFileManager(this IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation(options =>
                options.FileProviders.Add(new EmbeddedFileProvider(typeof(FileManagerComponent)
                    .GetTypeInfo().Assembly, "HGO.ASPNetCore.FileManager")));
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(IFileManagerCommandsProcessor), typeof(FileManagerCommandsProcessor));
        }

        /// <summary>Adds the file-manager embedded static files and session middleware.</summary>
        /// <param name="builder">The application's middleware pipeline.</param>
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

        /// <summary>Renders the file-manager JavaScript references.</summary>
        /// <param name="razorPage">The Razor page used to resolve the application path.</param>
        /// <param name="exclude">Optional names of script libraries to exclude.</param>
        /// <returns>An HTML string containing the script references.</returns>
        public static HtmlString RenderHgoFileManagerJavaScripts(this RazorPageBase razorPage, string[]? exclude = null)
        {
            var excludelist = new List<string>();
            if (exclude is { Length: > 0 })
            {
                excludelist.AddRange(exclude);
            }

            var basePath = razorPage.ViewContext.HttpContext.Request.PathBase.HasValue
                ? razorPage.ViewContext.HttpContext.Request.PathBase.Value!.TrimEnd('/')
                : "";
            var scripts = "";
            if (!excludelist.Any(p => p.ToLower() == "js.cookie"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/js.cookie/js.cookie.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "fontawesome"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/fontawesome/js/fontawesome.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "lozad.js"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/lozad.js/lozad.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "jsTree"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/jsTree/jstree.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "viselect"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/viselect/viselect.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "split.js"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/split.js/split.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "dropzone"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/dropzone/dropzone.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "context-js"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/context-js/context/context.min.js'></script>\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "toastify"))
            {
                scripts += $"<script src='{basePath}/hgofilemanager/toastify/toastify.js'></script>\r\n";
            }

            scripts += $"<script src='{basePath}/hgofilemanager/HgoFileManager.js'></script>\r\n";
            return new HtmlString(scripts);
        }

        /// <summary>Renders the file-manager CSS references and records the selected theme.</summary>
        /// <param name="razorPage">The Razor page used to resolve the application path and session.</param>
        /// <param name="darkMode">Whether to render the dark theme.</param>
        /// <param name="exclude">Optional names of style libraries to exclude.</param>
        /// <returns>An HTML string containing the style references.</returns>
        public static HtmlString RenderHgoFileManagerCss(this RazorPageBase razorPage, bool darkMode = false, string[]? exclude = null)
        {
            var excludelist = new List<string>();
            if (exclude is { Length: > 0 })
            {
                excludelist.AddRange(exclude);
            }

            var basePath = razorPage.ViewContext.HttpContext.Request.PathBase.HasValue
                ? razorPage.ViewContext.HttpContext.Request.PathBase.Value!.TrimEnd('/')
                : "";
            var styles = $"<script>window.HGOFM_BASEPATH = '{basePath}';</script>\r\n";
            if (!excludelist.Any(p => p.ToLower() == "jsTree"))
            {
                styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/jsTree/themes/default/style.min.css' type='text/css' />\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "dropzone"))
            {
                styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/dropzone/dropzone.css' type='text/css' />\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "fontawesome"))
            {
                styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/fontawesome/css/all.min.css' type='text/css' />\r\n";
            }
            if (!excludelist.Any(p => p.ToLower() == "context-js"))
            {
                if (darkMode)
                {
                    styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/context-js/context/skins/kali_dark.css' type='text/css' />\r\n";
                }
                else
                {
                    styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/context-js/context/skins/kali_light.css' type='text/css' />\r\n";
                }
            }
            if (!excludelist.Any(p => p.ToLower() == "toastify"))
            {
                styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/toastify/toastify.css' type='text/css' />\r\n";
            }

            styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/HgoFileManager.css' type='text/css' />\r\n";

            if (darkMode)
            {
                styles += $"<link rel='stylesheet' href='{basePath}/hgofilemanager/HgoFileManager-dark.css' type='text/css' />\r\n";
                razorPage.ViewContext.HttpContext.Session.SetString("HgoFileManagerTheme", "Dark");
            }

            return new HtmlString(styles);
        }
    }
}
