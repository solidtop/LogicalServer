using LogicalServer.Common.Messaging;
using LogicalServer.Core.Internal;

namespace LogicalServer.Core
{
    public class HubConnectionHandler<THub>(HubManager<THub> hubManager, IHubDispatcher<THub> dispatcher, ILogger<HubConnectionHandler<THub>> logger) : IConnectionHandler where THub : Hub
    {
        private readonly HubManager<THub> _hubManager = hubManager;
        private readonly IHubDispatcher<THub> _dispatcher = dispatcher;
        private readonly ILogger<HubConnectionHandler<THub>> _logger = logger;

        public async Task<HubConnection> OnConnectedAsync(HubConnection connection)
        {
            _logger.LogDebug("Client Connected to {HubName}", typeof(THub).Name);
            await _hubManager.OnConnectedAsync(connection);
            await _dispatcher.OnConnectedAsync(connection);

            return connection;
        }

        public async Task OnDisconnectedAsync(HubConnection connection, Exception? exception)
        {
            try
            {
                await _hubManager.OnDisconnectedAsync(connection);
                await _dispatcher.OnDisconnectedAsync(connection, exception);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching hub event");
                throw;
            }
            finally
            {
                _logger.LogDebug("Client Disconnected from {HubName}", typeof(THub).Name);
                connection.Close();
            }
        }

        public async Task DispatchMessageAsync(HubConnection connection, HubMessage message)
        {
            await _dispatcher.DispatchMessageAsync(connection, message);
        }

        private async Task SendCloseAsync(HubConnection connection, Exception? exception, bool allowReconnect)
        {
            var closeMessage = CloseMessage.Empty;

            if (exception is not null)
            {
                var errorMessage = ErrorMessageHelper.BuildErrorMessage("Connection closed with an error.", exception);
                closeMessage = new CloseMessage(errorMessage, allowReconnect);
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
