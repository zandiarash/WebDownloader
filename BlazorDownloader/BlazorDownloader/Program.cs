using Blazored.Toast;
using Microsoft.Extensions.FileProviders;
using MudBlazor.Services;
using BlazorDownloader.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using System.Net.Sockets;
using Microsoft.AspNetCore.Server.Kestrel.Core;


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

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = int.MaxValue; // or specify a larger size in bytes
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // or specify a larger size
});

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue; // Or specify a higher limit
});
// // Increase the multipart body length limit
// builder.Services.Configure<FormOptions>(options =>
// {
//     options.MultipartBodyLengthLimit = 536870912; // 512 MB
// });

// // Adjust the Kestrel server limits
// builder.WebHost.ConfigureKestrel(serverOptions =>
// {
//     serverOptions.Limits.MaxRequestBodySize = 536870912; // 512 MB
// });

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
    ServeUnknownFileTypes = true,
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
    RequestPath = requestPath,
});

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapDelete("/uploader", async (HttpRequest request) =>
{
    return Results.Ok();
});
// app.MapPost("/uploader", async (HttpRequest request) =>
// {
//     if (!request.HasFormContentType)
//     {
//         return Results.BadRequest("Invalid form content");
//     }

//     try
//     {
//         var form = await request.ReadFormAsync();
//         var file = form.Files[0];

//         if (file == null || file.Length == 0)
//             return Results.BadRequest("File not found or empty");

//         var tcpClient = new TcpClient("http://127.0.0.1", 9000);
//         using var networkStream = tcpClient.GetStream();
//         using var writer = new StreamWriter(networkStream) { AutoFlush = true };

//         await writer.WriteLineAsync(file.FileName);
//         await file.CopyToAsync(networkStream);

//         return Results.Ok(new { fileName = file.FileName, url = $"{request.GetDisplayUrl()}/{file.FileName}" });
//     }
//     catch (Exception ee)
//     {
//         return Results.BadRequest(ee.Message);
//     }
//});
// app.MapPost("/uploader", async (HttpRequest request) =>
// {
//     var maxRequestBodySizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
//     if (maxRequestBodySizeFeature != null)
//     {
//         maxRequestBodySizeFeature.MaxRequestBodySize = null;
//     }

//     if (!request.HasFormContentType)
//     {
//         return Results.BadRequest("Invalid form content");
//     }

//     try
//     {
//         var form = await request.ReadFormAsync();
//         var file = form.Files[0];

//         if (file == null || file.Length == 0)
//             return Results.BadRequest("File not found or empty");

//         var filePath = Path.Combine(downloadFolder, file.FileName);
//         using var stream = new FileStream(filePath, FileMode.Create);
//         await file.CopyToAsync(stream);

//         return Results.Ok(new { fileName = file.FileName, url = $"{request.GetDisplayUrl()}/{file.FileName}" });
//     }
//     catch (Exception ee)
//     {
//         return Results.BadRequest(ee.Message);
//     }
// });

app.MapPost("/uploader", async (HttpRequest request) =>
{
    var maxRequestBodySizeFeature = request.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (maxRequestBodySizeFeature != null)
    {
        maxRequestBodySizeFeature.MaxRequestBodySize = null;
    }

    if (!request.HasFormContentType)
    {
        return Results.BadRequest("Invalid form content");
    }

    try
    {
        var form = await request.ReadFormAsync();
        var file = form.Files[0];

        if (file == null || file.Length == 0)
            return Results.BadRequest("File not found or empty");

        var filePath = Path.Combine(downloadFolder, file.FileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return Results.Ok(new { fileName = file.FileName, url = $"{request.GetDisplayUrl()}/{file.FileName}" });
    }
    catch (Exception ee)
    {
        Console.WriteLine(ee.Message);
        return Results.BadRequest(ee.Message);
    }
});


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
}