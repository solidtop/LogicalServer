using LS.Common.Exceptions;
using LS.Common.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LS.Common.Messaging
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

        public HubMessage Parse(string data)
        {
            using var reader = new StringReader(data);
            using var jsonReader = new JsonTextReader(reader);

            return _serializer.Deserialize<HubMessage>(jsonReader) ?? throw new InvalidMessageException();
        }
    }
}
