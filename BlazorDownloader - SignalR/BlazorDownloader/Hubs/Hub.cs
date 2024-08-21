using Microsoft.AspNetCore.SignalR;
namespace BlazorDownloader.Hubs;

public class GlobalHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
