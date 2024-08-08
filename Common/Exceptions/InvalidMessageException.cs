namespace LS.Common.Exceptions
{
    public class InvalidMessageException : Exception
    {
        public InvalidMessageException() : base("Invalid message") { }
        public InvalidMessageException(string message) : base(message) { }
    }
}
