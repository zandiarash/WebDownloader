
using Microsoft.AspNetCore.Mvc;
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
    public double? processPercent { get; set; }
    public long downloadedSize { get; set; } = 0L;
    public long? downloadFileSize { get; set; } = 0L;
}

public interface IDownloadService
{
    public Task<bool> DownloadFromUrlAsync(Func<bool> stateChange, string downloadRootPath, string url, string fileName);
}
public class DownloadService : IDownloadService
{
    public static List<Download> fileNameUrlDownloading = new();
    public static List<string> fileNameUrlDownnloaded = new();
    public static async Task removeDownloadingItem(string url, string downloadRootPath ,bool removeFileAlso=false)
    {
        var item = fileNameUrlDownloading.FirstOrDefault(x => x.url == url);
        if (item != null)
        {
            item.cancellationTokenSource.Cancel();
            fileNameUrlDownloading.Remove(item);
            var FileNamePath = Path.Combine(downloadRootPath, item.fileName);

            if (removeFileAlso && File.Exists(FileNamePath))
                await new TaskFactory().StartNew(() =>
               {
                   bool deleted = false;
                   while (!deleted)
                   {
                       try
                       {
                           File.Delete(FileNamePath);
                       }
                       catch
                       {
                           Task.Delay(500);
                       }
                   }
               });
        }
    }
    public static void removeDownloadedItem(string fileName, string downloadRootPath)
    {
        fileNameUrlDownnloaded.Remove(fileName);
        var FileNamePath = Path.Combine(downloadRootPath, fileName);
        if (File.Exists(FileNamePath))
            File.Delete(FileNamePath);
    }

    public async Task<bool> DownloadFromUrlAsync(Func<bool> stateChange, string downloadRootPath, string url, string fileName)
    {
        if (fileNameUrlDownloading.Any(x => x.url == url)) { return false; }
        var thisDownload = new Download(url, fileName, new CancellationTokenSource());

        fileNameUrlDownloading.Add(thisDownload);

        string DestinationFilePath = Path.Combine(downloadRootPath, fileName);

        // Clearing last downloaded file
        removeDownloadedItem(fileName, DestinationFilePath);

        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, thisDownload.cancellationTokenSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP error: {response.StatusCode}");
                await removeDownloadingItem(thisDownload.url, downloadRootPath);
                stateChange();
                return false;
            }

            thisDownload.downloadFileSize = response.Content.Headers.ContentLength;

            using var contentStream = await response.Content.ReadAsStreamAsync(thisDownload.cancellationTokenSource.Token);
            using var fileStream = new FileStream(DestinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            var isMoreToRead = true;

            do
            {
                if (thisDownload.cancellationTokenSource.IsCancellationRequested)
                    break;
                var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    isMoreToRead = false;
                    continue;
                }

                await fileStream.WriteAsync(buffer, 0, bytesRead, thisDownload.cancellationTokenSource.Token);

                thisDownload.downloadedSize += bytesRead;
                thisDownload.processPercent = thisDownload.downloadedSize * 1d / (thisDownload.downloadFileSize ?? 0) * 100;
                stateChange();
            } while (isMoreToRead && !thisDownload.cancellationTokenSource.IsCancellationRequested);

            fileNameUrlDownnloaded.Add(fileName);
        }
        catch
        {
        }

        await removeDownloadingItem(url, downloadRootPath);
        stateChange();
        return true;
    }
}