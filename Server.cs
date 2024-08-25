using LogicalServer.Common.Messaging;
using LogicalServer.Core;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace LogicalServer
{
    public class Server(ConnectionHandlerResolver resolver, ILoggerFactory loggerFactory, ILogger<Server> logger)
    {
        private readonly ConnectionHandlerResolver _resolver = resolver;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
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
                    _ = Task.Run(() => HandleConnectionAsync(tcpClient), cancellationToken);
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

        private async Task HandleConnectionAsync(TcpClient tcpClient)
        {
            var stream = tcpClient.GetStream();
            var reader = PipeReader.Create(stream);
            var writer = PipeWriter.Create(stream);
            var transport = new HubTransport(reader, writer);
            var connection = new HubConnection(tcpClient, transport, _loggerFactory);

            try
            {
                var result = await connection.HandshakeAsync(_resolver);

                if (result.IsSuccess)
                {
                    var handler = result.Handler ?? throw new InvalidOperationException("Handler is null");
                    await handler.OnConnectedAsync(connection);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "An error occurred");
            }
            finally
            {
                await reader.CompleteAsync();
                tcpClient.Close();
                _logger.LogDebug("Connection closed.");
            }
        }
    }
}

