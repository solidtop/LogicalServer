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
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            return JsonConvert.DeserializeObject<HubMessage>(message, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
            }) ?? throw new InvalidMessageException();
        }

        public static string Process(HubMessage message)
        {
            DefaultContractResolver contractResolver = new()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            };

            return JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
            }) ?? throw new InvalidMessageException();
        }
    }
}
