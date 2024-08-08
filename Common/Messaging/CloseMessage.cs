namespace LS.Common.Messaging
{
    public class CloseMessage : HubMessage
    {
        public static readonly CloseMessage Empty = new(error: null, false);

        public string? Error { get; }
        public bool AllowReconnect { get; }

        public CloseMessage(string? error)
        {
            Error = error;
            Type = MessageType.Close;
        }

        public CloseMessage(string? error, bool allowReconnect)
        {
            Error = error;
            AllowReconnect = allowReconnect;
            Type = MessageType.Close;
        }
    }
}
