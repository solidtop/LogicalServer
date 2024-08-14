using LS.Common.Messaging;

namespace LS.Core.Internal
{
    internal sealed class HubDispatcher<THub>(
        IServiceScopeFactory scopeFactory,
        HubMethodInvoker<THub> methodInvoker,
        IHubContext<THub> hubContext
        ) : IHubDispatcher<THub> where THub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly HubMethodInvoker<THub> _methodInvoker = methodInvoker;
        private readonly IHubContext<THub> _hubContext = hubContext;

        public async Task OnConnectedAsync(HubConnection connection)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var hubActivator = scope.ServiceProvider.GetRequiredService<HubActivator<THub>>();
            var hub = hubActivator.Create();
            InitializeHub(hub, connection);
            await hub.OnConnectedAsync();
            hubActivator.Release(hub);
        }

        public async Task OnDisconnectedAsync(HubConnection connection, Exception? exception)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var hubActivator = scope.ServiceProvider.GetRequiredService<HubActivator<THub>>();
            var hub = hubActivator.Create();
            InitializeHub(hub, connection);
            await hub.OnDisconnectedAsync(exception);
            hubActivator.Release(hub);
        }

        public Task DispatchMessageAsync(HubConnection connection, HubMessage hubMessage)
        {
            return hubMessage switch
            {
                (HubInvocationMessage invocationMessage) => ProcessInvocation(connection, invocationMessage),
                _ => throw new NotSupportedException($"Received unsupported message: {hubMessage}"),
            };
        }

        private async Task ProcessInvocation(HubConnection connection, HubInvocationMessage message)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var hubActivator = scope.ServiceProvider.GetRequiredService<HubActivator<THub>>();
            var hub = hubActivator.Create();
            InitializeHub(hub, connection);

            try
            {
                var task = _methodInvoker.InvokeAsync(hub, message.MethodName, message.Arguments);
                await connection.WriteAsync(CompletionMessage.WithResult(message.InvocationId, task.Result));
            }
            catch (Exception ex)
            {
                var exception = ex.InnerException is null ? ex : ex.InnerException;
                var errorMessage = ErrorMessageHelper.BuildErrorMessage($"Failed to invoke {message.MethodName} due to an error.", exception);
                await connection.WriteAsync(CompletionMessage.WithError(message.InvocationId, errorMessage));
                throw;
            }
            finally
            {
                hubActivator.Release(hub);
            }
        }

        private void InitializeHub(Hub hub, HubConnection connection)
        {
            hub.Clients = new HubCallerClients(connection.Id, _hubContext.Clients);
            hub.Context = connection.HubCallerContext;
            hub.Sessions = _hubContext.Sessions;
        }
    }
}
