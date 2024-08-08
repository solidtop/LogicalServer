using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LS.Common.Messaging
{
    public class HubMessageConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(HubMessage).IsAssignableFrom(objectType);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var resolver = serializer.ContractResolver as DefaultContractResolver;
            var namingStrategy = resolver?.NamingStrategy ?? new DefaultNamingStrategy();

            JObject jsonObject = JObject.Load(reader);

            var messageType = GetProperty<MessageType>(nameof(HubMessage.Type), jsonObject, namingStrategy);

            HubMessage message = messageType switch
            {
                MessageType.Connection => CreateConnectionRequest(jsonObject, namingStrategy),
                MessageType.Invocation => CreateInvocationMessage(jsonObject, namingStrategy),
                _ => throw new JsonSerializationException($"Unknown message type: {messageType}")
            };

            serializer.Populate(jsonObject.CreateReader(), message);
            return message;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private static HubConnectionRequest CreateConnectionRequest(JObject jsonObject, NamingStrategy strategy)
        {
            var hubName = GetProperty<string>("HubName", jsonObject, strategy);

            return new HubConnectionRequest(hubName);
        }

        private static HubInvocationMessage CreateInvocationMessage(JObject jsonObject, NamingStrategy strategy)
        {
            var invocationId = GetProperty<string>("InvocationId", jsonObject, strategy);
            var methodName = GetProperty<string>("MethodName", jsonObject, strategy);
            var args = GetProperty<object[]>("Arguments", jsonObject, strategy);

            return new HubInvocationMessage(invocationId, methodName, args);
        }

        private static T GetProperty<T>(string propertyName, JObject jsonObject, NamingStrategy strategy)
        {
            var name = strategy.GetPropertyName(propertyName, hasSpecifiedName: false);
            var token = jsonObject[name];

            if (token is null || token.Type == JTokenType.Null)
            {
                throw new JsonSerializationException($"Property {name} is missing or null");
            }

            return token.ToObject<T>() ?? throw new JsonSerializationException("Error serializing message");
        }
    }
}
