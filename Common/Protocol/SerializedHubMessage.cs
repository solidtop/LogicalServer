using LogicalServer.Common.Messaging;

namespace LogicalServer.Common.Protocol
{
    public class SerializedHubMessage(HubMessage message)
    {
        public ReadOnlyMemory<byte> Data { get; } = HubProtocol.GetMessageBytes(message);
    }
}