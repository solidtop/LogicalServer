namespace LogicalServer.Sessions
{
    public class Session(string id)
    {
        public string Id { get; } = id;
        public HashSet<string> ClientIds { get; set; } = [];
        public int MaxClients { get; set; } = 10;

        public static Session Create()
        {
            string sessionId = Guid.NewGuid().ToString();
            return new Session(sessionId);
        }

        public static Session Create(string sessionId)
        {
            return new Session(sessionId);
        }
    }
}
