using LS.Common.Json;

namespace LS
{
    public class ServerOptions
    {
        public long MaximumReceiveMessageSize { get; set; } = 32 * 1024;
        public JsonOptions Json { get; set; } = new();
    }
}
