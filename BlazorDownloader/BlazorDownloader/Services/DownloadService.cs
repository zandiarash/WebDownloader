
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
    public Task<bool> DownloadFromUrlAsync(Func<Task<bool>> stateChange, string downloadRootPath, string url, string fileName);
    public Task<bool> PromptForDownloadAndDownloadAtLast(Func<Task<bool>> stateChange, string downloadRootPath, string url, string fileName);
}
public class DownloadService : IDownloadService
{
    public static List<Download> fileNameUrlDownloading = new();
    //public static List<Download> fileNameUrlDownloadingQueue = new();
    public static List<string> fileNameUrlDownnloaded = new();
    public async Task<bool> PromptForDownloadAndDownloadAtLast(Func<Task<bool>> stateChange, string downloadRootPath, string url, string fileName)
    {
        if (fileNameUrlDownloading.Count >= Program.downloadLimit)
        {
            var newDownload = new Download(url, fileName, new CancellationTokenSource());
            //fileNameUrlDownloadingQueue.Add(newDownload);
            return false;
        }
        return true;
    }
    public async Task<bool> DownloadFromUrlAsync(Func<Task<bool>> stateChange, string downloadRootPath, string url, string fileName)
    {
        if (fileNameUrlDownloading.Any(x => x.url == url)) { return false; }
        var thisDownload = new Download(url, fileName, new CancellationTokenSource());

        fileNameUrlDownloading.Add(thisDownload);

        string DestinationFilePath = Path.Combine(downloadRootPath, fileName);

        // Clearing last downloaded file
        await removeDownloadedItem(stateChange, fileName, DestinationFilePath);

        try
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, thisDownload.cancellationTokenSource.Token);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP error: {response.StatusCode}");
                await removeDownloadingItem(stateChange, thisDownload.url, downloadRootPath);
                //await stateChange();
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
                    return false;
                var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    isMoreToRead = false;
                    continue;
                }

                await fileStream.WriteAsync(buffer, 0, bytesRead, thisDownload.cancellationTokenSource.Token);

                thisDownload.downloadedSize += bytesRead;
                thisDownload.processPercent = thisDownload.downloadedSize * 1d / (thisDownload.downloadFileSize ?? 0) * 100;
                await stateChange();
            } while (isMoreToRead && !thisDownload.cancellationTokenSource.IsCancellationRequested);

            if (!thisDownload.cancellationTokenSource.IsCancellationRequested)
                fileNameUrlDownnloaded.Add(fileName);
        }
        catch
        {
        }

        await removeDownloadingItem(stateChange, url, downloadRootPath);
        await stateChange();
        return true;
    }

    public static async Task removeDownloadingItem(Func<Task<bool>> stateChange, string url, string downloadRootPath, bool removeFileAlso = false)
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
        await stateChange();
    }
    public static async Task<bool> removeDownloadedItem(Func<Task<bool>> stateChange, string fileName, string downloadRootPath)
    {
        fileNameUrlDownnloaded.Remove(fileName);
        var FileNamePath = Path.Combine(downloadRootPath, fileName);
        if (File.Exists(FileNamePath))
            File.Delete(FileNamePath);

        await stateChange();
        return true;
    }

    public static string byteToRealSize(long bytes)
    {
        if (bytes == 0L) return "";

        const long kilobyte = 1024;
        const long megabyte = kilobyte * 1024;
        const long gigabyte = megabyte * 1024;
        const long terabyte = gigabyte * 1024;

        if (bytes >= terabyte)
            return $"{(double)bytes / terabyte:0.##} TB";
        else if (bytes >= gigabyte)
            return $"{(double)bytes / gigabyte:0.##} GB";
        else if (bytes >= megabyte)
            return $"{(double)bytes / megabyte:0.##} MB";
        else
            return $"{(double)bytes / kilobyte:0.##} KB"; // default to KB if less than MB
    }

}