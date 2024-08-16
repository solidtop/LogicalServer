namespace LogicalServer.Core.Internal
{
    public class HubCallerContext(HubConnection connection)
    {
        private readonly HubConnection _connection = connection;

        public string ConnectionId => _connection.Id;
    }
}
