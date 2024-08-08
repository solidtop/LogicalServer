using LS.Common.Messaging;

namespace LS.Core
{
    public class HubManager<THub>(HubConnectionStore connections) where THub : Hub
    {
        private readonly HubConnectionStore _connections = connections;

        public Task OnConnectedAsync(HubConnection connection)
        {
            _connections.Add(connection);
            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync(HubConnection connection)
        {
            _connections.Remove(connection);
            return Task.CompletedTask;
        }

        public Task SendAllAsync(string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections(methodName, args, include: null, cancellationToken);
        }

        public Task SendAllExceptAsync(string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections(methodName, args, connection => !excludedConnectionIds.Contains(connection.Id), cancellationToken);
        }

        public Task SendConnectionsAsync(string methodName, object?[] args, IReadOnlyList<string> connectionIds, CancellationToken cancellationToken = default)
        {
            return SendToAllConnections(methodName, args, connection => connectionIds.Contains(connection.Id), cancellationToken);
        }

        private Task SendToAllConnections(string methodName, object?[] args, Func<HubConnection, bool>? include, CancellationToken cancellationToken = default)
        {
            List<Task> tasks = [];

            foreach (var connection in _connections)
            {
                if (include != null && !include(connection))
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var message = new InvocationMessage(methodName, args);

                var task = connection.WriteAsync(message, cancellationToken);

                if (!task.IsCompletedSuccessfully)
                {
                    tasks.Add(task.AsTask());
                }
                else
                {
                    task.GetAwaiter().GetResult();
                }
            }

            if (tasks.Count == 0)
            {
                return Task.CompletedTask;
            }

            return Task.WhenAll(tasks);
        }

        public Task SendConnectionAsync(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connectionId);

            var connection = _connections[connectionId];

            if (connection is null)
                return Task.CompletedTask;

            var message = new InvocationMessage(methodName, args);

            return connection.WriteAsync(message, cancellationToken).AsTask();
        }
    }
}
