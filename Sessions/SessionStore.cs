using System.Collections.Concurrent;

namespace LogicalServer.Sessions
{
    public class SessionStore
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = [];

        public Session AddSession()
        {
            var session = Session.Create();
            _sessions.TryAdd(session.Id, session);
            return session;
        }

        public Session GetOrAddSession(string sessionId)
        {
            return _sessions.GetOrAdd(sessionId, id => Session.Create(id));
        }

        public void RemoveSession(string sessionId)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        public IReadOnlyDictionary<string, Session> Sessions { get { return _sessions; } }

        public Session GetSession(string sessionId) => _sessions[sessionId];
        public bool SessionExists(string sessionId) => _sessions.TryGetValue(sessionId, out _);
    }
}
