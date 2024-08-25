using LogicalServer.Common;
using LogicalServer.Common.Messaging;
using System.Reflection;

namespace LogicalServer.Core.Internal
{
    internal sealed class HubDispatcher<THub>(IServiceScopeFactory scopeFactory, IHubContext<THub> hubContext) : IHubDispatcher<THub> where THub : Hub
    {
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
        private readonly IHubContext<THub> _hubContext = hubContext;
        private readonly Dictionary<string, HubMethodDescriptor> _methodCache = CacheMethods();

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
            switch (hubMessage)
            {
                case HubInvocationMessage invocationMessage:
                    return ProcessInvocation(connection, invocationMessage);
                case CloseMessage:
                    connection.Abort();
                    break;
                default:
                    throw new NotSupportedException($"Received unsupported message: {hubMessage}");
            }

            return Task.CompletedTask;
        }

        public IReadOnlyList<Type> GetParameterTypes(string methodName)
        {
            if (!_methodCache.TryGetValue(methodName, out var descriptor))
            {
                throw new HubException("Method does not exist.");
            }

            return descriptor.ParameterTypes;
        }

        private async Task ProcessInvocation(HubConnection connection, HubInvocationMessage message)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var hubActivator = scope.ServiceProvider.GetRequiredService<HubActivator<THub>>();
            var hub = hubActivator.Create();
            InitializeHub(hub, connection);

            try
            {
                var task = InvokeMethodAsync(hub, message.MethodName, message.Arguments);

                if (task != null && message.InvocationId != null)
                {
                    await connection.WriteAsync(CompletionMessage.WithResult(message.InvocationId, task.Result));
                }
            }
            catch (AggregateException ex)
            {
                if (message.InvocationId is null)
                {
                    return;
                }

                var exception = ex.InnerException is null ? ex : ex.InnerException;
                var errorMessage = ErrorMessageHelper.BuildErrorMessage($"Failed to invoke {message.MethodName} due to an error.", exception);
                await connection.WriteAsync(CompletionMessage.WithError(message.InvocationId, errorMessage));
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

        private async Task<object?> InvokeMethodAsync(Hub hub, string methodName, object?[] args)
        {
            if (!_methodCache.TryGetValue(methodName, out var method))
            {
                throw new InvalidOperationException($"Method '{methodName}' not found on hub '{typeof(THub).Name}'.");
            }

            return await method.Invoker(hub, args);
        }

        private static Dictionary<string, HubMethodDescriptor> CacheMethods()
        {
            Dictionary<string, HubMethodDescriptor> methodCache = [];
            var hubType = typeof(THub);

            foreach (var method in hubType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if ((method.ReturnType == typeof(Task) ||
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                    && !method.IsVirtual)
                {
                    methodCache[method.Name] = new HubMethodDescriptor(method);
                }
            }

            return methodCache;
        }
    }
}
