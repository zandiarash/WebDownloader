
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
}

public interface IDownloadService
{
    public Task<bool> DownloadFromUrlAsync(Func<bool> stateChange, string downloadRootPath, string url, string fileName);
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
    public static void removeDownloadedItem(string fileName, string downloadRootPath)
    {
        // Clearing last downloaded fle
        fileNameUrlDownnloaded.Remove(fileName);
        var FileNamePath = Path.Combine(downloadRootPath, fileName);
        if (File.Exists(FileNamePath))
            File.Delete(FileNamePath);
    }

    public async Task<bool> DownloadFromUrlAsync(Func<bool> stateChange, string downloadRootPath, string url, string fileName)
    {
        if (fileNameUrlDownnloading.Any(x => x.url == url)) { return false; }
        var thisDownload = new Download(url, fileName, new CancellationTokenSource());

        fileNameUrlDownnloading.Add(thisDownload);

        string fileNamePath = Path.Combine(downloadRootPath, fileName);

        removeDownloadedItem(fileName, fileNamePath);


        // Downloadning
        //var fileInfo = new FileInfo(fileNamePath);
        //try
        //{
        //    //HttpClient client = new HttpClient();
        //    //using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, thisDownload.cancellationTokenSource.Token);
        //    //// You must use as stream to have control over buffering and number of bytes read/received
        //    //using var stream = await response.Content.ReadAsStreamAsync();
        //    //// Read/process bytes from stream as appropriate
        //    //// Calculated by you based on how many bytes you have read.  Likely incremented within a loop.
        //    //long bytesRecieved = 0;//...

        //    //long? totalBytes = response.Content.Headers.ContentLength;
        //    ////double? percentComplete = (double)bytesRecieved / totalBytes;
        //    //thisDownload.processPercent = (double)bytesRecieved / totalBytes;
        //    //// Do what you want with `percentComplete`
        //    //stateChange();
        //    //using var fileStream = fileInfo.OpenWrite();
        //    //await stream.CopyToAsync(fileStream);
        //}
        //catch { }





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

        //Writing to file
        var stream = await response.Content.ReadAsStreamAsync();
        var fileInfo = new FileInfo(fileNamePath);
        using var fileStream = fileInfo.OpenWrite();
        await stream.CopyToAsync(fileStream);


        fileNameUrlDownnloaded.Add(fileName);
        removeDownloadingItem(url);

        stateChange();
        return true;
    }
}
