using Blazored.Toast;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using BlazorDownloader.Models;
using Microsoft.AspNetCore.Http.Extensions;


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
builder.Services.AddSingleton<GlobalState>();

var app = builder.Build();
ServiceProviderHelper.Configure(app.Services);

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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
              Path.Combine(Directory.GetCurrentDirectory(), "node_modules")),
    RequestPath = "/node_modules"
});

app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = requestPath
});

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapDelete("/uploader", async (HttpRequest request) =>
{
    return Results.Ok();
});
app.MapPost("/uploader", async (HttpRequest request) =>
{
    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Invalid form content");
    }

    var form = await request.ReadFormAsync();
    var file = form.Files[0];

    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("File not found or empty");
    }

    // Save the file to a directory on the server
    var filePath = Path.Combine(downloadFolder, file.FileName);

    // Save the file
    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    return Results.Ok(new { fileName = file.FileName, url = $"{request.GetDisplayUrl()}/{file.FileName}" });
});

//app.MapPost("/uploader/inform", async (HttpContext context) =>
//{
//    return;
//    using var scope = builder.Services.BuildServiceProvider().CreateScope();
//    var stateService = context.RequestServices.GetRequiredService<GlobalState>();
//    await stateService.Act();
//});

app.Run();


public partial class Program
{
    public static UserModel thisUser = new();
    public static IConfigurationRoot? configurations;
    public static string downloadFolder = "Downloadables";
    public static string downloadRootPath = string.Empty;
    public static byte downloadLimit
    {
        get; set;
    }
    public static string? Credentials
    {
        get; set;
    }
    public static string cacheNameUsers = "users";

    // public static string downloadRootPath = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Downloadables")).Root;
}