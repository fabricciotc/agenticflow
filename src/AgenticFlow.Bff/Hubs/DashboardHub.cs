using Microsoft.AspNetCore.SignalR;

namespace AgenticFlow.Bff.Hubs;

public class DashboardHub : Hub
{
    public async Task RequestUpdate()
    {
        await Clients.Caller.SendAsync("status_update", new { status = "ok" });
    }

    public async Task ChatSend(string message)
    {
        await Clients.All.SendAsync("chat_message", new { message });
    }
}
