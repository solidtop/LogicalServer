namespace LogicalServer.Common.Messaging
{
    public class HandshakeResponseMessage : HubMessage
    {
        public static readonly HandshakeResponseMessage Empty = new(error: null);

        public string? Error { get; }

        public HandshakeResponseMessage(string? error)
        {
            Error = error;
            Type = MessageType.Handshake;
        }
    }
}
