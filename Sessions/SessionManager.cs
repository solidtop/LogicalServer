using LogicalServer.Common.Exceptions;
using LogicalServer.Hubs;
using Microsoft.Extensions.Options;

namespace LogicalServer.Sessions
{
    public class SessionManager(SessionStore sessionStore, HubClientStore clientStore, IOptions<SessionOptions> options, ILogger<SessionManager> logger)
    {
        private readonly SessionStore _sessionStore = sessionStore;
        private readonly HubClientStore _clientStore = clientStore;
        private readonly IOptions<SessionOptions> _options = options;
        private readonly ILogger<SessionManager> _logger = logger;

        public Task AddToSessionAsync(string clientId, string sessionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var session = _sessionStore.GetOrAddSession(sessionId);

            lock (session)
            {
                if (session.ClientIds.Count >= _options.Value.MaxClients)
                {
                    _logger.LogWarning("Cannot add client {clientId} to session {sessionId}: Maximum number of clients reached", clientId, sessionId);
                    return Task.CompletedTask;
                }

                session.ClientIds.Add(clientId);
            }

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
                    }
                }
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
    }
}