using LogicalServer.Common.Messaging;

namespace LogicalServer.Core
{
    public interface IHubDispatcher<THub> where THub : Hub
    {
        Task OnConnectedAsync(HubConnection connection);
        Task OnDisconnectedAsync(HubConnection connection, Exception? exception);
        Task DispatchMessageAsync(HubConnection connection, HubMessage hubMessage);
    }
}
