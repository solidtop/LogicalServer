using LogicalServer.Common.Exceptions;

namespace LogicalServer.Sessions
{
    public class SessionFinder(SessionStore store)
    {
        private readonly SessionStore _store = store;

        public Session FindSession(string clientId)
        {
            var session = _store.Sessions.FirstOrDefault(session => session.Key.Contains(clientId)).Value;

            if (session != null)
                return session;

            if (_store.Sessions.Count == 0)
                _store.AddSession();

            bool sessionIsFound = false;

            while (!sessionIsFound)
            {
                session = SearchForSession();

                if (session is null)
                    _store.AddSession();
                else
                    sessionIsFound = true;
            }

            if (session is null)
                throw new SessionNotFoundException();

            return session;
        }

        private Session? SearchForSession()
        {
            return _store.Sessions.Values.FirstOrDefault(session => session.ClientIds.Count < session.MaxClients);
        }
    }
}
