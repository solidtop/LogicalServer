namespace LogicalServer.Core
{
    public interface IConnectionHandler
    {
        Task OnConnectedAsync(HubConnection connection);
    }
}
