namespace LogicalServer.Common.Messaging
{
    public class HandshakeRequestMessage : HubMessage
    {
        public string HubName { get; }

        public HandshakeRequestMessage(string hubName)
        {
            HubName = hubName;
            Type = MessageType.Handshake;
        }
    }
}
