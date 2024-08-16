using LogicalServer.Common.Json;
using LogicalServer.Core.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Buffers;
using System.Text;

namespace LogicalServer.Common.Messaging
{
    public class HubMessageParser
    {
        private readonly JsonSerializer _serializer;

        public HubMessageParser(JsonNamingStrategyProvider namingStrategyProvider)
        {
            _serializer = new JsonSerializer
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = namingStrategyProvider.GetNamingStrategy(),
                }
            };

            _serializer.Converters.Add(new HubMessageConverter());
        }

        public bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out HubMessage? message)
        {
            if (!TextMessageParser.TryParseMessage(ref buffer, out var payload))
            {
                message = null;
                return false;
            }

            message = Parse(payload);

            return message != null;
        }

        private HubMessage? Parse(ReadOnlySequence<byte> buffer)
        {
            string data = Encoding.UTF8.GetString(buffer);

            using var reader = new StringReader(data);
            using var jsonReader = new JsonTextReader(reader);

            return _serializer.Deserialize<HubMessage>(jsonReader);
        }
    }
}
