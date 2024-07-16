using System.Text;

namespace LogicalServer.Hubs
{
    public class HubClientSender(IReadOnlyDictionary<string, HubClient> clients, string route)
    {
        private readonly IReadOnlyDictionary<string, HubClient> _clients = clients;
        private readonly string _route = route;

        private Task SendAsync(string methodName, object?[] data)
        {
            ArgumentNullException.ThrowIfNull(methodName, nameof(methodName));

            var message = new HubMessage(_route, methodName, data);
            var messageStr = HubMessageParser.Parse(message);
            var buffer = Encoding.UTF8.GetBytes(messageStr);

            foreach (var client in _clients.Values)
            {
                client.Stream.WriteAsync(buffer, 0, buffer.Length);
            }

            return Task.CompletedTask;
        }

        public Task SendAsync(string methodName, object? arg1)
        {
            return SendAsync(methodName, [arg1]);
        }

        public Task SendAsync(string methodName, object? arg1, object? arg2)
        {
            return SendAsync(methodName, [arg1, arg2]);
        }

        public Task SendAsync(string methodName, object? arg1, object? arg2, object? arg3)
        {
            return SendAsync(methodName, [arg1, arg2, arg3]);
        }

        public Task SendAsync(string methodName, object? arg1, object? arg2, object? arg3, object? arg4)
        {
            return SendAsync(methodName, [arg1, arg2, arg3, arg4]);
        }

        public Task SendAsync(string methodName, object? arg1, object? arg2, object? arg3, object? arg4, object? arg5)
        {
            return SendAsync(methodName, [arg1, arg2, arg3, arg4, arg5]);
        }
    }
}
