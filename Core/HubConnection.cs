using LS.Common.Messaging;
using LS.Core.Internal;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Text;

namespace LS.Core
{
    public class HubConnection
    {
        public HubConnection(TcpClient tcpClient)
        {
            Id = Guid.NewGuid().ToString();
            TcpClient = tcpClient;
            HubCallerContext = new HubCallerContext(this);

            TcpClient.EnableKeepAlive();
        }

        internal string Id { get; }
        internal TcpClient TcpClient { get; }
        internal HubCallerContext HubCallerContext { get; }

        public async ValueTask WriteAsync(HubMessage message, CancellationToken cancellationToken = default)
        {
            if (!TcpClient.Connected)
                return;

            var serializedMessage = JsonConvert.SerializeObject(message);
            var stream = TcpClient.GetStream();
            var buffer = Encoding.UTF8.GetBytes(serializedMessage);
            await stream.WriteAsync(buffer, cancellationToken);
        }

        public void Close() => TcpClient.Close();
    }
}