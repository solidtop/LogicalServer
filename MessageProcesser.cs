using LogicalServer.Common.Exceptions;
using LogicalServer.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LogicalServer
{
    public class MessageProcesser
    {
        public static HubMessage Process(string message)
        {
            return JsonConvert.DeserializeObject<HubMessage>(message, GetSerializerSettings()) ?? throw new InvalidMessageException();
        }

        public static string Process(HubMessage message)
        {
            return JsonConvert.SerializeObject(message, GetSerializerSettings()) ?? throw new InvalidMessageException();
        }

        public static string Process(HubError error)
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
