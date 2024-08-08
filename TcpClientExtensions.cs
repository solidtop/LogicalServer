using System.Net.Sockets;

namespace LS
{
    public static class TcpClientExtensions
    {
        private const int keepAliveTime = 1;
        private const int keepAliveInterval = 1;
        private const int keepAliveRetryCount = 2;

        public static void EnableKeepAlive(this TcpClient client)
        {
            var socket = client.Client;

            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, keepAliveTime);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, keepAliveInterval);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, keepAliveRetryCount);
        }
    }
}
