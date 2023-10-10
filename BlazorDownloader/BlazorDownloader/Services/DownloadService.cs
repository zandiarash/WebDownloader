
public interface IDownloadService
{
    public Task DownloadFromUrlAsync(string url, string fileName);
}
public class DownloadService : IDownloadService
{
    public static Dictionary<string, string> fileNameUrlDownnloading = new();
    public static List<string> fileNameUrlDownnloaded = new();
    public async Task DownloadFromUrlAsync(string url, string fileName)
    {
        if(fileNameUrlDownnloading.ContainsKey(url)) { return; }

        HttpResponseMessage response = null;
        string fileNamePath = Path.Combine(Program.downloadRootPath, fileName);
        using var client = new HttpClient();
        try
        {
            response = await client.GetAsync(url);
        }
        catch (Exception ex) { }
        if (response==null || !response.IsSuccessStatusCode)
        {
            fileNameUrlDownnloading.Remove(url);
            return;
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var fileInfo = new FileInfo(fileNamePath);

        if (File.Exists(fileNamePath))
            File.Delete(fileNamePath);

        using var fileStream = fileInfo.OpenWrite();
        await stream.CopyToAsync(fileStream);

        fileNameUrlDownnloading.Remove(url);
        fileNameUrlDownnloaded.Add(fileName);
    }
}
