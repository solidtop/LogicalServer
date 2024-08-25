using LogicalServer.Common.Messaging;
using LogicalServer.Core.Internal;
using Microsoft.Extensions.Options;

namespace LogicalServer.Core
{
    public class HubConnectionHandler<THub>(
        HubManager<THub> hubManager,
        IHubDispatcher<THub> dispatcher,
        IOptions<HubOptions> options,
        ILogger<HubConnectionHandler<THub>> logger
        ) : IConnectionHandler where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly IHubDispatcher<THub> _dispatcher = dispatcher;
        private readonly HubOptions _options = options.Value;
        private readonly ILogger<HubConnectionHandler<THub>> _logger = logger;

        public async Task OnConnectedAsync(HubConnection connection)
        {
            try
            {
                await _hubManager.OnConnectedAsync(connection);
                await RunHubAsync(connection);
            }
            finally
            {
                await _hubManager.OnDisconnectedAsync(connection);
            }
        }

        private async Task RunHubAsync(HubConnection connection)
        {
            try
            {
                await _dispatcher.OnConnectedAsync(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching hub event: OnConnectedAsync");
                await SendCloseAsync(connection, ex, false);
                return;
            }

            try
            {
                await DispatchMessagesAsync(connection);
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error processing message");
                await OnDisconnectedAsync(connection, ex);
                return;
            }

            await OnDisconnectedAsync(connection, null);
        }

        private async Task OnDisconnectedAsync(HubConnection connection, Exception? exception)
        {
            await SendCloseAsync(connection, exception, allowReconnect: false);

            try
            {
                await _dispatcher.OnDisconnectedAsync(connection, exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching hub event");
                throw;
            }
        }

        private async Task DispatchMessagesAsync(HubConnection connection)
        {
            var cancellationToken = connection.CancellationToken;
            var input = connection.Transport.Input;
            var binder = new InvocationBinder<THub>(_dispatcher);
            var maxMessageSize = _options.MaximumReceiveMessageSize;

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await input.ReadAsync(cancellationToken);
                var buffer = result.Buffer;

                try
                {
                    if (result.IsCanceled)
                    {
                        break;
                    }

                    if (buffer.IsEmpty)
                    {
                        continue;
                    }

                    while (!buffer.IsEmpty)
                    {
                        var segment = buffer;
                        var overLimit = false;

                        if (segment.Length > maxMessageSize)
                        {
                            segment = segment.Slice(segment.Start, maxMessageSize);
                            overLimit = true;
                        }

                        if (HubProtocol.TryParseMessage(ref segment, binder, out var message))
                        {
                            if (message is not null)
                            {
                                await _dispatcher.DispatchMessageAsync(connection, message);
                            }
                        }
                        else if (overLimit)
                        {
                            throw new InvalidDataException($"The maximum message size of {maxMessageSize}B was exceeded");
                        }
                        else
                        {
                            break;
                        }

                        buffer = buffer.Slice(segment.Start);
                    }

                    if (result.IsCompleted)
                    {
                        if (!buffer.IsEmpty)
                        {
                            throw new InvalidDataException("Connection closed with incomplete message");
                        }

                        break;
                    }
                }
                finally
                {
                    input.AdvanceTo(buffer.Start, buffer.End);
                }
            }
        }

        private async Task SendCloseAsync(HubConnection connection, Exception? exception, bool allowReconnect)
        {
            var closeMessage = CloseMessage.Empty;

            if (exception is not null)
            {
                var errorMessage = ErrorMessageHelper.BuildErrorMessage("Connection closed with an error.", exception);
                closeMessage = new CloseMessage(errorMessage, allowReconnect);
            }
            else if (allowReconnect)
            {
                closeMessage = new CloseMessage(error: null, allowReconnect);
            }

            try
            {
                await connection.WriteAsync(closeMessage);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error sending close");
            }
        }
    }
}
