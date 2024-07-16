using LogicalServer.Common.Exceptions;
using LogicalServer.Hubs;

namespace LogicalServer.Sessions
{
    public class SessionManager(SessionStore sessionStore, HubClientStore clientStore, ILogger<SessionManager> logger)
    {
        private readonly SessionStore _sessionStore = sessionStore;
        private readonly HubClientStore _clientStore = clientStore;
        private readonly ILogger<SessionManager> _logger = logger;

        public Task AddToSessionAsync(string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var session = _sessionStore.GetOrAddSession(sessionId);

            lock (session)
            {
                session.ClientIds.Add(clientId);
            }

            _logger.LogInformation("Client added to session {sessionId}", sessionId);

            return Task.CompletedTask;
        }

        public Task RemoveFromSessionAsync(string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_sessionStore.Sessions.TryGetValue(sessionId, out var session))
            {
                lock (session)
                {
                    session.ClientIds.Remove(clientId);
                    if (session.ClientIds.Count == 0)
                    {
                        _sessionStore.RemoveSession(sessionId);
                        _logger.LogInformation("Session {sessionId} removed", sessionId);
                    }
                }

                _logger.LogInformation("Client removed from session {sessionId}", sessionId);
            }

            return Task.CompletedTask;
        }

        public IReadOnlyDictionary<string, HubClient> GetClientsInSession(string sessionId)
        {
            if (_sessionStore.Sessions.TryGetValue(sessionId, out var session))
            {
                return session.ClientIds
                    .Where(_clientStore.Clients.ContainsKey)
                    .ToDictionary(clientId => clientId, clientId => _clientStore.Clients[clientId]);
            }

            throw new SessionNotFoundException(sessionId);
        }

        public Session? FindSession(string clientId)
        {
            return _sessionStore.Sessions.FirstOrDefault(session => session.Value.ClientIds.Contains(clientId)).Value;
        }
    }
}