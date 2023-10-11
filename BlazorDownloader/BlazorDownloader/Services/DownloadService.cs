
using System;
using System.Threading;

public class Download
{
    public Download(string _url, string _fileName, CancellationTokenSource _cancellationTokenSource)
    {
        url = _url;
        fileName = _fileName;
        cancellationTokenSource = _cancellationTokenSource;
    }
    public string url { get; set; }
    public string fileName { get; set; }
    public CancellationTokenSource cancellationTokenSource { get; set; }

}

public interface IDownloadService
{
    public Task<bool> DownloadFromUrlAsync(string downloadRootPath, string url, string fileName);
}
public class DownloadService : IDownloadService
{
    public static List<Download> fileNameUrlDownnloading = new();
    public static List<string> fileNameUrlDownnloaded = new();
    public static void removeDownloadingItem(string url)
    {
        var item = fileNameUrlDownnloading.FirstOrDefault(x => x.url == url);
        if (item != null)
        {
            item.cancellationTokenSource.Cancel();
            fileNameUrlDownnloading.Remove(item);
        }
    }
    public static void removeDownloadedItem(string fileName,string downloadRootPath)
    {
        // Clearing last downloaded fle
        fileNameUrlDownnloaded.Remove(fileName);
        var FileNamePath = Path.Combine(downloadRootPath, fileName);
        if (File.Exists(FileNamePath))
            File.Delete(FileNamePath);
    }

    public async Task<bool> DownloadFromUrlAsync(string downloadRootPath,string url, string fileName)
    {
        if (fileNameUrlDownnloading.Any(x => x.url == url)) { return false; }
        var thisDownload = new Download(url, fileName, new CancellationTokenSource());

        fileNameUrlDownnloading.Add(thisDownload);

        string fileNamePath = Path.Combine(downloadRootPath, fileName);

        removeDownloadedItem(fileName, fileNamePath);

        // Downloadning
        HttpResponseMessage response = null;
        try
        {
            response = await new HttpClient().GetAsync(url, thisDownload.cancellationTokenSource.Token);
        }
        catch { }
        if (response == null || !response.IsSuccessStatusCode)
        {
            removeDownloadingItem(url);
            return false;
        }

        // Writing to file
        var stream = await response.Content.ReadAsStreamAsync();
        var fileInfo = new FileInfo(fileNamePath);
        using var fileStream = fileInfo.OpenWrite();
        await stream.CopyToAsync(fileStream);

        fileNameUrlDownnloaded.Add(fileName);
        removeDownloadingItem(url);

        return true;
    }
}
