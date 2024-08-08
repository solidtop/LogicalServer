using LS.Core;
using System.Collections.Concurrent;

namespace LogicalServer.Core.Internal
{
    public class Session
    {
        private readonly ConcurrentDictionary<string, HubConnection> _connections = [];

        public ConcurrentDictionary<string, HubConnection> Connections => _connections;

        public void AddOrUpdateConnection(HubConnection connection)
        {
            _connections.AddOrUpdate(connection.Id, connection, (_, __) => connection);
        }

        public bool RemoveConnection(string connectionId)
        {
            return _connections.TryRemove(connectionId, out _);
        }

        public bool IsEmpty => _connections.IsEmpty;
    }
}
