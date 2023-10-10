using Microsoft.Extensions.FileProviders;


public class Program
{
    public static IConfigurationRoot configurations;

    public static string downloadFolder = "Downloadables";
    public static string downloadRootPath = string.Empty;
    // public static string downloadRootPath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Downloadables")).Root;
    public static Dictionary<string, string> fileNameUrlDownnloading = new();
    private static void Main(string[] args)
    {

        configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddDirectoryBrowser();
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
        builder.Services.AddSingleton<IDownloadService, DownloadService>();
        var app = builder.Build();

        downloadRootPath = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, downloadFolder)).Root;

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStaticFiles();
        //app.UseStaticFiles(new StaticFileOptions
        //{
        //    FileProvider = new PhysicalFileProvider(
        //           Path.Combine(builder.Environment.ContentRootPath, "Downloadables")),
        //    RequestPath = "/Downloadables"
        //});

        var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Downloadables"));
        var requestPath = "/Downloadables";

        // Enable displaying browser links.
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = requestPath
        });

        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = fileProvider,
            RequestPath = requestPath
        });


        app.UseRouting();

        app.MapRazorPages();

        app.Run();
    }
}

