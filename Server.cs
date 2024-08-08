using LS.Common.Messaging;
using LS.Core;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace LS
{
    public class Server(
        IOptions<ServerOptions> options,
        ConnectionHandlerResolver resolver,
        HubMessageParser messageParser,
        ILogger<Server> logger
        )
    {
        private readonly ServerOptions _options = options.Value;
        private readonly ConnectionHandlerResolver _resolver = resolver;
        private readonly HubMessageParser _messageParser = messageParser;
        private readonly ILogger<Server> _logger = logger;
        private TcpListener? _listener;

        public void Start(string ipAdress, int port)
        {
            _listener = new TcpListener(IPAddress.Parse(ipAdress), port);
            _listener.Start();
            _logger.LogInformation("Starting server on port {port}...", port);
        }

        public async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            if (_listener is null)
                throw new NullReferenceException("TcpListener");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var tcpClient = await _listener.AcceptTcpClientAsync(cancellationToken);

                    if (tcpClient.Connected)
                    {
                        _ = Task.Run(() => ReadMessagesAsync(tcpClient, cancellationToken), cancellationToken);
                    }
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

        private async Task ReadMessagesAsync(TcpClient tcpClient, CancellationToken cancellationToken)
        {
            var connection = new HubConnection(tcpClient);
            HubConnectionResult result;

            try
            {
                result = await ReadConnectionRequestAsync(connection, cancellationToken);
                await SendConnectionResponseAsync(connection, null, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading connection request");
                await SendConnectionResponseAsync(connection, ex, cancellationToken);
                connection.Close();
                return;
            }

            var handler = result.Handler;

            await handler.OnConnectedAsync(connection);

            var bufferSize = _options.MaximumReceiveMessageSize;
            var buffer = new byte[bufferSize];
            var stream = tcpClient.GetStream();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int bytesRead = await stream.ReadAsync(buffer, cancellationToken);

                    if (bytesRead == 0)
                    {
                        break;
                    }

                    string data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    var message = _messageParser.Parse(data);

                    await handler.DispatchMessageAsync(connection, message);
                }
                catch (IOException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reading message");
                }
            }

            await handler.OnDisconnectedAsync(connection, null);
        }

        private async Task<HubConnectionResult> ReadConnectionRequestAsync(HubConnection connection, CancellationToken cancellationToken)
        {
            var buffer = new byte[64];
            var stream = connection.TcpClient.GetStream();

            await stream.ReadAsync(buffer, cancellationToken);

            string data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            var message = _messageParser.Parse(data);

            if (message is HubConnectionRequest connectionRequest)
            {
                var handler = _resolver.GetHandler(connectionRequest.HubName)
                    ?? throw new InvalidOperationException($"Handler not found for hub: {connectionRequest.HubName}");

                return new HubConnectionResult(handler);
            }
            else
            {
                throw new InvalidOperationException("Invalid connection request");
            }
        }

        private async Task SendConnectionResponseAsync(HubConnection connection, Exception? exception, CancellationToken cancellationToken)
        {
            var response = HubConnectionResponse.Empty;

            if (exception is not null)
            {
                response = new HubConnectionResponse(exception.Message);
            }

            try
            {
                await connection.WriteAsync(response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending connection response");
            }
        }
    }
}
