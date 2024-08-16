using LogicalServer.Common.Messaging;
using LogicalServer.Core.Internal;
using System.Collections.Concurrent;

namespace LogicalServer.Core
{
    public class HubManager<THub>(HubConnectionStore connections, SessionStore sessions) where THub : Hub
    {
        private readonly HubConnectionStore _connections = connections;
        private readonly SessionStore _sessions = sessions;

        public Task OnConnectedAsync(HubConnection connection)
        {
            _connections.Add(connection);
            return Task.CompletedTask;
        }

        public Task OnDisconnectedAsync(HubConnection connection)
        {
            lock (connection.SessionIds)
            {
                foreach (var sessionId in connection.SessionIds)
                {
                    _sessions.Remove(connection.Id, sessionId);
                }
            }

            _connections.Remove(connection);

            return Task.CompletedTask;
        }

        public Task AddToSessionAsync(string connectionId, string sessionId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connectionId);
            ArgumentNullException.ThrowIfNull(sessionId);

            var connection = _connections[connectionId];

            if (connection is null)
                return Task.CompletedTask;

            lock (connection.SessionIds)
            {
                if (!connection.SessionIds.Add(sessionId))
                {
                    return Task.CompletedTask;
                }

                _sessions.Add(connection, sessionId);
            }

            return Task.CompletedTask;
        }

        public Task RemoveFromSessionAsync(string connectionId, string sessionId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(connectionId);
            ArgumentNullException.ThrowIfNull(sessionId);

            var connection = _connections[connectionId];

            if (connection is null)
                return Task.CompletedTask;

            lock (connection.SessionIds)
            {
                if (!connection.SessionIds.Remove(sessionId))
                {
                    return Task.CompletedTask;
                }

                _sessions.Remove(connectionId, sessionId);
            }

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

        public Task SendConnectionAsync(string connectionId, string methodName, object?[] args, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(connectionId);

            var connection = _connections[connectionId];

            if (connection is null)
                return Task.CompletedTask;

            var message = new InvocationMessage(methodName, args);

            return connection.WriteAsync(message, cancellationToken).AsTask();
        }

        public Task SendSessionsAsync(IReadOnlyList<string> sessionIds, string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            List<Task> tasks = [];
            var message = new InvocationMessage(methodName, args);

            foreach (var sessionId in sessionIds)
            {
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new InvalidOperationException("Cannot send to empty sessionId");
                }

                var session = _sessions[sessionId];

                if (session is null)
                    continue;

                SendToSessionConnections(session.Connections, null, ref tasks, ref message, cancellationToken);
            }

            return Task.WhenAll(tasks);
        }

        public Task SendSessionAsync(string sessionId, string methodName, object?[] args, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);

            var session = _sessions[sessionId];

            if (session is null)
                return Task.CompletedTask;

            List<Task> tasks = [];
            var message = new InvocationMessage(methodName, args);

            SendToSessionConnections(session.Connections, null, ref tasks, ref message, cancellationToken);

            return Task.WhenAll(tasks);
        }

        public Task SendSessionExcept(string sessionId, string methodName, object?[] args, IReadOnlyList<string> excludedConnectionIds, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sessionId);

            var session = _sessions[sessionId];

            if (session is null)
                return Task.CompletedTask;

            List<Task> tasks = [];
            var message = new InvocationMessage(methodName, args);

            SendToSessionConnections(session.Connections, connection => !excludedConnectionIds.Contains(connection.Id), ref tasks, ref message, cancellationToken);

            return Task.WhenAll(tasks);
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

        private static void SendToSessionConnections(
            ConcurrentDictionary<string, HubConnection> connections,
            Func<HubConnection, bool>? include,
            ref List<Task> tasks,
            ref InvocationMessage message,
            CancellationToken cancellationToken
            )
        {
            foreach (var connection in connections)
            {
                if (include != null && !include(connection.Value))
                {
                    continue;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var task = connection.Value.WriteAsync(message, cancellationToken);

                if (!task.IsCompletedSuccessfully)
                {
                    tasks.Add(task.AsTask());
                }
                else
                {
                    task.GetAwaiter().GetResult();
                }
            }
        }
    }
}
