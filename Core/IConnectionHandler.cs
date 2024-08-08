using LS.Common.Messaging;

namespace LS.Core
{
    public interface IConnectionHandler
    {
        Task<HubConnection> OnConnectedAsync(HubConnection connection);
        Task OnDisconnectedAsync(HubConnection connection, Exception? exception);
        Task DispatchMessageAsync(HubConnection connection, HubMessage message);
    }
}
