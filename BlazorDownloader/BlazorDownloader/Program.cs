using BlazorDownloader.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.FileProviders;

configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();


var builder = WebApplication.CreateBuilder(args);
downloadRootPath = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, downloadFolder)).Root;

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<IDownloadService, DownloadService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, downloadFolder));
var requestPath = $"/{downloadFolder}";

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();


public partial class Program
{
    public static IConfigurationRoot configurations;
    public static string downloadFolder = "Downloadables";
    public static string downloadRootPath = string.Empty;
    // public static string downloadRootPath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Downloadables")).Root;
}