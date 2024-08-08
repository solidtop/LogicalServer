namespace LS.Common.Messaging
{
    public class HubConnectionRequest : HubMessage
    {
        public string HubName { get; }

        public HubConnectionRequest(string hubName)
        {
            HubName = hubName;
            Type = MessageType.Connection;
        }

        public override string ToString()
        {
            return $"HubName: {HubName}";
        }
    }
}
