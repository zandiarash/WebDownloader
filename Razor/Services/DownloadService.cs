
public interface IDownloadService
{
    public Task DownloadFromUrlAsync(string url,string fileName);
}
public class DownloadService : IDownloadService
{
    public async Task DownloadFromUrlAsync(string url,string fileName)
    {
        string fileNamePath = Path.Combine(Program.downloadRootPath, fileName);
        using var client = new HttpClient();

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Program.fileNameUrlDownnloading.Remove(url);
            return;
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var fileInfo = new FileInfo(fileNamePath);

        if (File.Exists(fileNamePath))
            File.Delete(fileNamePath);

        using var fileStream = fileInfo.OpenWrite();
        await stream.CopyToAsync(fileStream);

        Program.fileNameUrlDownnloading.Remove(url);
    }
}
