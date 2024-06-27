namespace LogicalServer.Hubs
{
    public class HubContext(string clientId)
    {
        public string ClientId { get; } = clientId;
    }
}
