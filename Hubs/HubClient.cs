using System.Net.Sockets;

namespace LogicalServer.Hubs
{
    public class HubClient
    {
        public required string Id { get; set; }
        public required Socket Socket { get; set; }
        public required NetworkStream Stream { get; set; }
    }
}
