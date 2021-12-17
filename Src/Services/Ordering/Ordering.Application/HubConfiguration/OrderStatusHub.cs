using Microsoft.AspNetCore.SignalR;

namespace Ordering.Application.HubConfiguration
{

    public class OrderStatusHub : Hub
    {
        // For now we don't need any implementation since we are only sending notification about the changed status from the Commands with IHubContext to Client
        // Only one way communication
    }
}
