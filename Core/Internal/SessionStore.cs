using LS.Core;
using System.Collections;
using System.Collections.Concurrent;

namespace LogicalServer.Core.Internal
{
    public class SessionStore : IEnumerable<Session>
    {
        private readonly ConcurrentDictionary<string, Session> _sessions = [];

        public Session? this[string id]
        {
            get
            {
                _sessions.TryGetValue(id, out var session);
                return session;
            }
        }

        public void Add(HubConnection connection, string sessionId)
        {
            _sessions.AddOrUpdate(sessionId, _ => AddConnectionToSession(connection, new Session()),
                (key, oldSession) =>
                {
                    AddConnectionToSession(connection, oldSession);
                    return oldSession;
                });
        }

        public void Remove(string connectionId, string sessionId)
        {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                if (session.RemoveConnection(connectionId) && session.IsEmpty)
                {
                    _sessions.TryRemove(connectionId, out _);
                }
            }
        }

        private static Session AddConnectionToSession(HubConnection connection, Session session)
        {
            session.AddOrUpdateConnection(connection);
            return session;
        }

        public IEnumerator<Session> GetEnumerator()
        {
            return _sessions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
