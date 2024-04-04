using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using HGO.ASPNetCore.FileManager.CommandsProcessor;
using HGO.ASPNetCore.FileManager.ViewComponents;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;

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
    }
}
