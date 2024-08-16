namespace LogicalServer.Core
{
    public class ConnectionHandlerResolver(IServiceProvider serviceProvider)
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly Dictionary<string, Type> _handlerTypes = [];

        public void AddHandler<THub, TConnectionHandler>()
            where THub : Hub
            where TConnectionHandler : HubConnectionHandler<THub>
        {
            var hubName = typeof(THub).Name;
            _handlerTypes[hubName] = typeof(TConnectionHandler);
        }

        public IConnectionHandler? GetHandler(string hubName)
        {
            if (_handlerTypes.TryGetValue(hubName, out var handlerType))
            {
                return _serviceProvider.GetRequiredService(handlerType) as IConnectionHandler;
            }

            return null;
        }
    }
}
