using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using HGO.ASPNetCore.FileManager.CommandsProcessor;
using HGO.ASPNetCore.FileManager.ViewComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace HGO.ASPNetCore.FileManager
{
    public static class Extensions
    {
        public static void AddHgoFileManager(this IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation(c => c.FileProviders.Add(new EmbeddedFileProvider(typeof(FileManagerComponent)
                .GetTypeInfo().Assembly, "HGO.ASPNetCore.FileManager")));
            services.AddSession();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped(typeof(IFileManagerCommandsProcessor), typeof(FileManagerCommandsProcessor));
            services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement, UnicodeRanges.LatinExtendedA }));
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
                excludelist.AddRange(exclude.Select(e => e.ToLower())); // Normalize to lowercase for easier comparison
            }

            // Convert the exclusion list to a JavaScript array
            var excludeArrayJs = string.Join(", ", excludelist.Select(e => $"'{e}'"));

            var scripts = $@"
    <script>
        window.scriptsLoaded = false;
        var excludeList = [{excludeArrayJs}]; // Exclusion list passed from C#

        if (typeof jQuery === 'undefined') {{var script = document.createElement('script');
            script.src = 'hgofilemanager/jquery/dist/jquery.min.js';
            script.onload = function() {{loadLibraries();
            }};
            document.head.appendChild(script);
        }} else {{loadLibraries();
        }}

        function loadLibraries() {{
            var librariesToLoad = [
                {{ src: 'hgofilemanager/js.cookie/js.cookie.js', callback: onJsCookieLoaded }},
                {{ src: 'hgofilemanager/fontawesome/js/fontawesome.min.js', callback: onFontAwesomeLoaded }},
                {{ src: 'hgofilemanager/lozad.js/lozad.min.js', callback: onLozadLoaded }},
                {{ src: 'hgofilemanager/jstree/jstree.min.js', callback: onJstreeLoaded }},
                {{ src: 'hgofilemanager/viselect/viselect.js', callback: onViselectLoaded }},
                {{ src: 'hgofilemanager/split.js/split.js', callback: onSplitLoaded }},
                {{ src: 'hgofilemanager/dropzone/dropzone.js', callback: onDropzoneLoaded }},
                {{ src: 'hgofilemanager/context-js/context/context.min.js', callback: onContextJsLoaded }},
                {{ src: 'hgofilemanager/toastify/toastify.js', callback: onToastifyLoaded }}
            ];

            loadNextLibrary(librariesToLoad, 0);
        }}

        function loadNextLibrary(libraries, index) {{
            if (index < libraries.length) {{
                var library = libraries[index];
                // Check if the library is excluded
                if (!excludeList.includes(library.src.split('/').pop().toLowerCase())) {{
                    var script = document.createElement('script');
                    script.src = library.src;
                    script.onload = function() {{
                        if (library.callback) {{
                            library.callback();
                        }}
                        loadNextLibrary(libraries, index + 1); // Load the next library
                    }};
                    document.head.appendChild(script);
                }} else {{
                    loadNextLibrary(libraries, index + 1); // Skip this library and load the next
                }}
            }} else {{
                window.scriptsLoaded = true; // All libraries loaded
                onAllLibrariesLoaded(); // You can call a function here that needs all libraries
            }}
        }}

        // Callbacks for individual libraries
        function onJsCookieLoaded() {{
            console.log('js-cookie loaded');
        }}

        function onFontAwesomeLoaded() {{
            console.log('Font Awesome loaded');
        }}

        function onLozadLoaded() {{
            console.log('Lozad.js loaded');
        }}

        function onJstreeLoaded() {{
            console.log('jsTree loaded');
        }}

        function onViselectLoaded() {{
            console.log('viSelect loaded');
        }}

        function onSplitLoaded() {{
            console.log('split.js loaded');
        }}

        function onDropzoneLoaded() {{
            console.log('Dropzone.js loaded');
        }}

        function onContextJsLoaded() {{
            console.log('context-js loaded');
        }}

        function onToastifyLoaded() {{
            console.log('Toastify.js loaded');
        }}

        function onAllLibrariesLoaded() {{
            console.log('All libraries loaded');
            // This is where you can execute any code that depends on all libraries being loaded
        }}
    </script>";

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
            if (!excludelist.Any(p => p.ToLower() == "jstree"))
            {
                styles +=
                    "<link rel='stylesheet' href='hgofilemanager/jstree/themes/default/style.min.css' type='text/css' />\r\n";
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
