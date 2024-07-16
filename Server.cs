using LogicalServer.Common.Exceptions;
using LogicalServer.Hubs;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LogicalServer
{
    public class Server(HubClientStore clientStore, HubManager hubManager, IExceptionHandler exceptionHandler, ILogger<Server> logger)
    {
        private readonly HubClientStore _clientStore = clientStore;
        private readonly HubManager _hubManager = hubManager;
        private readonly IExceptionHandler _exceptionHandler = exceptionHandler;
        private readonly ILogger<Server> _logger = logger;
        private TcpListener? _listener;

        public void Listen(string ipAdress, int port, Action callback)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAdress), port);
            _listener.Start();
            callback();
        }

        public async Task AcceptClientsAsync(CancellationToken stoppingToken)
        {
            if (_listener is null)
            {
                throw new Exception("TcpListener is null");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(stoppingToken);
                    _ = Task.Run(() => HandleClientAsync(tcpClient, stoppingToken), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting client");
                }
            }

            _listener.Stop();
            _logger.LogInformation("Server stopped listening");
        }

        private async Task HandleClientAsync(TcpClient tcpClient, CancellationToken stoppingToken)
        {
            var client = ConnectClient(tcpClient);
            var buffer = new byte[1024];

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await client.Stream.ReadAsync(buffer, stoppingToken);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string incomingMessage = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    var message = HubMessageParser.Parse(incomingMessage);

                    await _exceptionHandler.InvokeAsync(client.Stream, async () =>
                    {
                        await _hubManager.RouteMessageAsync(message, client);
                    });
                }
                catch (Exception ex)
                {
                    DisconnectClient(tcpClient, client.Id, ex);
                    return;
                }
            }

            DisconnectClient(tcpClient, client.Id, null);
        }

        private HubClient ConnectClient(TcpClient tcpClient)
        {
            var client = _clientStore.AddClient(tcpClient);
            _hubManager.OnConnectedAsync();
            NotifyClient(client);
            _logger.LogInformation("Client connected");

            return client;
        }

        private void DisconnectClient(TcpClient tcpClient, string clientId, Exception? ex)
        {
            tcpClient.Close();
            _clientStore.RemoveClient(clientId);
            _hubManager.OnDisconnectedAsync(clientId, ex);
            _logger.LogInformation("Client disconnected");
        }

        private static Task NotifyClient(HubClient client)
        {
            var message = new HubMessage("/", "on_connected", []);
            var messageStr = HubMessageParser.Parse(message);
            var buffer = Encoding.UTF8.GetBytes(messageStr);
            client.Stream.WriteAsync(buffer, 0, buffer.Length);

            return Task.CompletedTask;
        }
    }
}
