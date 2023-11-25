using BlazorDownloader.Hubs;
using Blazored.Toast;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Identity;
using System.Net;
using MudBlazor.Services;
using BlazorDownloader.Models;

configurations = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

downloadLimit = Convert.ToByte(configurations["DownloadLimit"]);
Credentials = configurations["Credentials"] ?? "";

var builder = WebApplication.CreateBuilder(args);

//var connectionString = builder.Configuration.GetConnectionString("DbContextConnection") ?? throw new InvalidOperationException("Connection string 'DbContextConnection' not found.");
builder.Services.AddMudServices();

downloadRootPath = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, downloadFolder)).Root;

builder.Services.AddBlazoredToast();


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());

builder.Services.AddSingleton<IDownloadService, DownloadService>();
builder.Services.AddSingleton<HttpClient>();
builder.Services.AddSignalR();


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
app.MapHub<GlobalHub>("/GlobalHub");
app.MapFallbackToPage("/_Host");

app.Run();


public partial class Program
{
    public static UserModel thisUser = new UserModel();
    public static IConfigurationRoot? configurations;
    public static string downloadFolder = "Downloadables";
    public static string downloadRootPath = string.Empty;
    public static byte downloadLimit { get; set; }
    public static string? Credentials { get; set; }
    public static string cacheNameUsers = "users";

    // public static string downloadRootPath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Downloadables")).Root;
}