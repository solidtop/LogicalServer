namespace LogicalServer.Session
{
    public class Session(string id)
    {
        public string Id { get; } = id;
        public HashSet<string> ClientIds { get; set; } = [];
    }
}
