using System.Collections;
using System.Collections.Concurrent;

namespace LogicalServer.Core
{
    public class HubConnectionStore : IEnumerable<HubConnection>
    {
        private readonly ConcurrentDictionary<string, HubConnection> _connections = [];

        public HubConnection? this[string id]
        {
            get
            {
                _connections.TryGetValue(id, out var connection);
                return connection;
            }
        }

        public void Add(HubConnection connection) => _connections.TryAdd(connection.Id, connection);
        public void Remove(HubConnection connection) => _connections.TryRemove(connection.Id, out _);

        public IEnumerator<HubConnection> GetEnumerator()
        {
            return _connections.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
