namespace LogicalServer.Common.Exceptions
{
    public class SessionNotFoundException : NotFoundException
    {

        public SessionNotFoundException() : base("Session not found") { }

        public SessionNotFoundException(string id) : base($"Session not found: {id}") { }
    }
}
