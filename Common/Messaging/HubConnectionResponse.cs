namespace LogicalServer.Common.Messaging
{
    public class HubConnectionResponse : HubMessage
    {
        public static readonly HubConnectionResponse Empty = new(error: null);

        public string? Error { get; }

        public HubConnectionResponse(string? error)
        {
            Error = error;
            Type = MessageType.Connection;
        }
    }
}
