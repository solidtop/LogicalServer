using System.Collections.Concurrent;
using System.Net.Sockets;

namespace LogicalServer.Hubs
{
    public class HubClientStore
    {
        private readonly ConcurrentDictionary<string, HubClient> _clients = [];

        public HubClient AddClient(TcpClient tcpClient)
        {
            string clientId = Guid.NewGuid().ToString();

            var client = new HubClient
            {
                Id = clientId,
                Socket = tcpClient.Client,
                Stream = tcpClient.GetStream(),
            };

            _clients.TryAdd(clientId, client);

            return client;
        }

        public void RemoveClient(string clientId)
        {
            _clients.TryRemove(clientId, out _);
        }

        public IReadOnlyDictionary<string, HubClient> Clients => _clients;
    }
}
