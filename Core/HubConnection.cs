using LogicalServer.Common.Messaging;
using LogicalServer.Common.Protocol;
using LogicalServer.Core.Internal;
using System.Net.Sockets;

namespace LogicalServer.Core
{
    public class HubConnection
    {
        private const long MaxMessageSize = 32_768;

        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly ILogger _logger;

        internal string Id { get; }
        internal TcpClient TcpClient { get; }
        internal HubCallerContext HubCallerContext { get; }
        internal HashSet<string> SessionIds { get; } = [];
        internal HubTransport Transport { get; }
        internal CancellationToken CancellationToken { get; }

        public HubConnection(TcpClient tcpClient, HubTransport transport, ILoggerFactory loggerFactory)
        {
            Id = Guid.NewGuid().ToString();
            TcpClient = tcpClient;
            HubCallerContext = new HubCallerContext(this);
            Transport = transport;
            CancellationToken = _cancellationTokenSource.Token;

            _logger = loggerFactory.CreateLogger<HubConnection>();

            TcpClient.EnableKeepAlive();
        }

        internal async Task<HandshakeResult> HandshakeAsync(ConnectionHandlerResolver handlerResolver)
        {
            try
            {
                var input = Transport.Input;

                while (!CancellationToken.IsCancellationRequested)
                {
                    var result = await input.ReadAsync(CancellationToken);

                    var buffer = result.Buffer;
                    var consumed = buffer.Start;
                    var examined = buffer.End;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            _logger.LogDebug("Handshake was canceled");
                            await WriteAsync(new HandshakeResponseMessage("Handshake was canceled"), CancellationToken);
                            break;
                        }

                        if (buffer.IsEmpty)
                        {
                            continue;
                        }

                        var segment = buffer;
                        var overLimit = false;

                        if (buffer.Length > MaxMessageSize)
                        {
                            segment = segment.Slice(segment.Start, MaxMessageSize);
                            overLimit = true;
                        }

                        if (HandshakeParser.TryParseRequestMessage(ref segment, out var message))
                        {
                            if (message is null)
                            {
                                continue;
                            }

                            consumed = segment.Start;
                            examined = consumed;

                            var handler = handlerResolver.GetHandler(message.HubName);

                            if (handler is null)
                            {
                                _logger.LogDebug("Handler not found for hub {Hub}", message.HubName);
                                await WriteAsync(new HandshakeResponseMessage($"Hub with name: {message.HubName} not found"), CancellationToken);
                                break;
                            }

                            input.AdvanceTo(buffer.Start, buffer.End);

                            _logger.LogDebug("Handshake complete");

                            await WriteAsync(HandshakeResponseMessage.Empty, CancellationToken);

                            return HandshakeResult.Success(handler);
                        }
                        else if (overLimit)
                        {
                            _logger.LogDebug("The maximum message size of {MaxMessageSize}B was exceeded while parsing the Handshake.", MaxMessageSize);
                            await WriteAsync(new HandshakeResponseMessage("Handshake was canceled."), CancellationToken);
                            break;
                        }

                        if (result.IsCompleted)
                        {
                            _logger.LogDebug("Failed connection handshake.");
                            break;
                        }
                    }
                    finally
                    {
                        input.AdvanceTo(consumed, examined);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Handshake was canceled");
                await WriteAsync(new HandshakeResponseMessage("Handshake was canceled"), CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed connection handshake.");
                var errorMessage = ErrorMessageHelper.BuildErrorMessage("An unexpected error occurred during connection handshake.", ex);
                await WriteAsync(new HandshakeResponseMessage(errorMessage), CancellationToken);
            }

            return HandshakeResult.Fail();
        }

        public async ValueTask WriteAsync(HubMessage message, CancellationToken cancellationToken = default)
        {
            if (!TcpClient.Connected)
                return;

            try
            {
                HubProtocol.WriteMessage(message, Transport.Output);

                var result = await Transport.Output.FlushAsync(cancellationToken);

                if (result.IsCanceled)
                {
                    _logger.LogDebug("Write operation was canceled");
                    return;
                }

                if (result.IsCompleted)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while writing the message.");
                throw;
            }
        }

        public async ValueTask WriteAsync(SerializedHubMessage message, CancellationToken cancellationToken = default)
        {
            if (!TcpClient.Connected)
                return;

            try
            {
                var buffer = message.Data;

                var result = await Transport.Output.WriteAsync(buffer, cancellationToken);

                if (result.IsCanceled)
                {
                    _logger.LogDebug("Write operation was canceled");
                    return;
                }

                if (result.IsCompleted)
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while writing the message.");
                throw;
            }
        }

        internal void Close() => TcpClient.Close();

        internal void Abort()
        {
            try
            {
                _cancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while aborting the connection.");
            }
        }
    }
}