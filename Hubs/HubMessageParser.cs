using LogicalServer.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LogicalServer.Hubs
{
    internal class HubMessageParser
    {
        public static HubMessage Parse(string message)
        {
            return JsonConvert.DeserializeObject<HubMessage>(message, GetSerializerSettings()) ?? throw new InvalidMessageException();
        }

        public static string Parse(HubMessage message)
        {
            return JsonConvert.SerializeObject(message, GetSerializerSettings()) ?? throw new InvalidMessageException();
        }

        public static string Parse(HubError error)
        {
            return JsonConvert.SerializeObject(error, GetSerializerSettings()) ?? throw new InvalidMessageException();
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            return new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
            };
        }
    }
}
