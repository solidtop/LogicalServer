using LogicalServer.Common.Json;

namespace LogicalServer
{
    public class ServerOptions
    {
        public long MaximumReceiveMessageSize { get; set; } = 32 * 1024;
        public JsonOptions Json { get; set; } = new();
    }
}
