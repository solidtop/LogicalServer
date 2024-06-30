using LogicalServer.Common.Exceptions;
using LogicalServer.Hubs;
using System.Collections.Concurrent;

namespace LogicalServer.Session
{
    public class SessionManager(HubClientStore clientStore, ILogger<SessionManager> logger)
    {
        private readonly HubClientStore _clientStore = clientStore;
        private readonly ILogger<SessionManager> _logger = logger;
        private readonly ConcurrentDictionary<string, Session> _sessions = [];

        public Task AddToSessionAsync(string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var session = _sessions.GetOrAdd(sessionId, id => new Session(id));

            lock (session)
            {
                session.ClientIds.Add(clientId);
            }

            _logger.LogInformation("Client {clientId} added to session {sessionId}", clientId, sessionId);

            return Task.CompletedTask;
        }

        public Task RemoveFromSessionAsync(string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_sessions.TryGetValue(sessionId, out var session))
            {
                lock (session)
                {
                    session.ClientIds.Remove(clientId);
                    if (session.ClientIds.Count == 0)
                    {
                        _sessions.TryRemove(sessionId, out _);
                        _logger.LogInformation("Session {sessionId} removed", sessionId);
                    }
                }

                _logger.LogInformation("Client {clientId} removed from session {sessionId}", clientId, sessionId);
            }

            return Task.CompletedTask;
        }

        public Session? FindSession(string clientId)
        {
            try
            {
                return _sessions.First(session => session.Value.ClientIds.Contains(clientId)).Value;
            }
            catch
            {
                return null;
            }
        }

        public IDictionary<string, HubClient> GetClientsInSession(string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                return session.ClientIds
                    .Where(clientId => _clientStore.Clients.ContainsKey(clientId))
                    .ToDictionary(clientId => clientId, clientId => _clientStore.Clients[clientId]);
            }

            throw new SessionNotFoundException(sessionId);
        }
    }
}
