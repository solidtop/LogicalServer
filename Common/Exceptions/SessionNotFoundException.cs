namespace LogicalServer.Common.Exceptions
{
    public class SessionNotFoundException : Exception
    {

        public SessionNotFoundException() : base("Session not found") { }

        public SessionNotFoundException(string id) : base($"Session not found: {id}") { }
    }
}
