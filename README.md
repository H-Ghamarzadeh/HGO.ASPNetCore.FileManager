# HGO.ASPNetCore.FileManager (Free & Open Source File Explorer for ASP.Net Core 6+)
HGO.ASPNetCore.FileManager is a free, open source, feature rich and easy to use file explorer/manager component for ASP.Net Core 6 and above with MIT license!

[![NuGet version (HGO.ASPNetCore.FileManager)](https://img.shields.io/nuget/v/HGO.ASPNetCore.FileManager)](https://www.nuget.org/packages/HGO.ASPNetCore.FileManager/)
![NuGet Downloads](https://img.shields.io/nuget/dt/Hgo.ASPNetCore.FileManager?style=flat&color=%23238636)

# **[Online Demo](https://filemanager.ghamarzadeh.com/)**

![HGO.ASPNetCore.FileManager](https://github.com/H-Ghamarzadeh/HGO.ASPNetCore.FileManager/blob/master/HGO.ASPNetCore.FileManager.png?raw=true "HGO.ASPNetCore.FileManager")

## Features:
-  Multi Language Support
-  Enum Member Access for type safety
-  Manage server's files from client side
-  Copy & cut & paste functionality
-  Compress & extract archive files (Rar, Zip, Tar, Tar.GZip, Tar.BZip2, Tar.LZip, Tar.XZ, GZip, 7Zip)
-  Download & upload
-  Rename & delete files/folders
-  Edit text based files with full feature code editor ([CodeMirror](https://codemirror.net/)) (e.g.: .css, .html, .js, ...)
-  Create new file/folder
-  Cross platform (Compatible with Windows & Linux & macOS file system)
-  Search
-  Ability to enable/disable features
-  Ability to control disk space usage
-  and more ...

![HGO.ASPNetCore.FileManager Light Mode](https://github.com/H-Ghamarzadeh/HGO.ASPNetCore.FileManager/blob/master/Light-min.png?raw=true "HGO.ASPNetCore.FileManager Light Mode")
![HGO.ASPNetCore.FileManager Dark Mode](https://github.com/H-Ghamarzadeh/HGO.ASPNetCore.FileManager/blob/master/Dark-min.png?raw=true "HGO.ASPNetCore.FileManager Dark Mode")

## How to Install:
At first you should install  [HGO.ASPNetCore.FileManager from NuGet](https://www.nuget.org/packages/HGO.ASPNetCore.FileManager):
```
Install-Package HGO.ASPNetCore.FileManager
```
Or via the .NET Core command line interface:

```cs
dotnet add package HGO.ASPNetCore.FileManager
```
Either commands, from Package Manager Console or .NET Core CLI, will download and install HGO.ASPNetCore.FileManager and all required dependencies.
Now you need to add HGO.ASPNetCore.FileManager to the ASP.NET Core services container. Open `Program.cs` and insert the marked lines into `Program.cs` file:
```cs
using HGO.ASPNetCore.FileManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// HGO.AspNetCore.FileManager -------
builder.Services.AddHgoFileManager();
//-----------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// HGO.AspNetCore.FileManager -------
app.UseHgoFileManager();
//-----------------------------------

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```
Now you need to create an `Action Method` to handle server side operations, so open (or create) a `Controller` class and add the following action method:
```cs
[HttpPost, HttpGet]
public async Task<IActionResult> HgoApi(string id, string command, string parameters, IFormFile file)
{
    return await _processor.ProcessCommandAsync(id, command, parameters, file);
}
```
Also you need to inject `IFileManagerCommandsProcessor` to you controller contractor method with IoC, so edit your controller contractor method like below:
```cs
private readonly IFileManagerCommandsProcessor _processor;

public HomeController(IFileManagerCommandsProcessor processor)
{
    _processor = processor;
}
```
Now you can add `HGO.ASPNetCore.FileManager` component view to any razor page or view you want:
```cs
<div style="height: 550px; margin-bottom:20px">
   @await Component.InvokeAsync("FileManagerComponent", new FileManagerModel()
    {
        Id = "FM1", //an application-wide unique ID
        RootFolder = AppDomain.CurrentDomain.BaseDirectory, //your desired path on server
        ApiEndPoint = Url.Action("HgoApi"), //Url of previously created action method
        Config = new FileManagerConfig() // othe configuration 
    })
</div>
```
Also you need to reference HGO.ASPNetCore.FileManager JavaScript and CSS files to your view:
```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    
    @*HGO.AspNetCore.FileManager Styles*@
    @this.RenderHgoFileManagerCss(true)
    @*---------------------------------*@
</head>
<body>
    <div class="container">
        <main role="main" class="pb-3">
             @await Component.InvokeAsync("FileManagerComponent", new FileManagerModel()
              {
                   Id = "FM1",
                   RootFolder = AppDomain.CurrentDomain.BaseDirectory,
                   ApiEndPoint = Url.Action("HgoApi"),
                   Config = new FileManagerConfig()
                   {
                        DisabledFunctions = new HashSet<Command>()
                        {
                        Command.Delete,
                        },
                       Language = new TurkishLanguage(),
                   }
              })
        </main>
    </div>

    @*HGO.AspNetCore.FileManager depends on jQuery, so you need to add jQuery reference before*@
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    @*----------------------------------*@

    @*HGO.AspNetCore.FileManager Scripts*@
    @this.RenderHgoFileManagerJavaScripts()
    @*----------------------------------*@
</body>
</html>
```
For more information please check the following sample projects:
- [ASP.Net Core MVC](https://github.com/H-Ghamarzadeh/HGO.ASPNetCore.FileManager/tree/master/test/HGO.ASPNetCore.FileManager.Test)
- [ASP.Net Core Razor Pages](https://github.com/H-Ghamarzadeh/HGO.ASPNetCore.FileManager/tree/master/test/HGO.ASPNetCore.FileManager.RazorPages.Test)

## See installation guide on YouTube:
[![Hgo.ASPNetCore.FileManager Installation guide in ASP.Net Core MVC project](https://i.ytimg.com/vi/_1bZYUQm3wc/hq720.jpg)](https://www.youtube.com/watch?v=_1bZYUQm3wc)
[![Hgo.ASPNetCore.FileManager Installation guide in ASP.Net Core Razor Pages project](https://i.ytimg.com/vi/kDlHLdVtrMc/hq720.jpg)](https://www.youtube.com/watch?v=kDlHLdVtrMc)

### Note:
HGO.ASPNetCore.FileManager depends on jQuery library, so you need reference jQuery library before calling `RenderHgoFileManagerJavaScripts()`

### Third-party JS libraries which I used in this project:
- [context-js](https://github.com/heapoverride/context-js)
- [dropzone](https://github.com/dropzone/dropzone)
- [Font Awesome](https://github.com/FortAwesome/Font-Awesome)
- [js-cookie](https://github.com/js-cookie/js-cookie)
- [jsTree](https://github.com/vakata/jstree)
- [Lozad.js](https://github.com/ApoorvSaxena/lozad.js)
- [Split.js](https://github.com/nathancahill/split)
- [toastify-js](https://github.com/apvarun/toastify-js)
- [viSelection](https://github.com/simonwep/selection)

Thanks to all!

