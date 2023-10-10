using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DownloadEngine.Pages;

public class IndexModel : PageModel
{
    private readonly IDownloadService _downloadService;
    public Dictionary<string, string> fileNameUrlDownnloading = new();
    public List<string> fileNameUrlDownnloaded = new();

    public IndexModel(IDownloadService downloadService)
    {
        downloadService = _downloadService;
        fileNameUrlDownnloading = Program.fileNameUrlDownnloading.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        var files = new DirectoryInfo(Program.downloadRootPath).GetFiles();
        foreach (var item in files)
            fileNameUrlDownnloaded.Add(item.Name);
    }

    public IWebHostEnvironment Env { get; }

    public void OnGet()
    {
    }
    public async Task<IActionResult> OnPost()
    {
        var link = Request.Form["Link"].FirstOrDefault()?.ToString();
        if (string.IsNullOrEmpty(link))
            return Page();

        var fileName = new Uri(link).Segments.Last();
        Program.fileNameUrlDownnloading.Add(link, fileName);

        if (!string.IsNullOrEmpty(link))
            new TaskFactory().StartNew(async () =>
            {
                await _downloadService.DownloadFromUrlAsync(link, fileName);
            });

        return RedirectToPage("/index");
    }

    // [HttpGet("download")]
    // public IActionResult GetBlobDownload([FromQuery] string link)
    // {
    //     var net = new System.Net.WebClient();
    //     var data = net.DownloadData(link);
    //     var content = new System.IO.MemoryStream(data);
    //     var contentType = "APPLICATION/octet-stream";
    //     var fileName = "something.bin";
    //     return File(content, contentType, fileName);
    // }
}
