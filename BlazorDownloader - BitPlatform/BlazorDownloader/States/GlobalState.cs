
public class GlobalState
{
    public List<Func<Task>> OnDownloadReceived = new();
    public async Task Act()
    {
        foreach (var task in OnDownloadReceived)
            await task.Invoke();
    }
}

public static class ServiceProviderHelper
{
    public static IServiceProvider ServiceProvider
    {
        get; set;
    }

    public static void Configure(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }
}
