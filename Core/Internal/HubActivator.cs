namespace LS.Core.Internal
{
    internal sealed class HubActivator<THub>(IServiceProvider serviceProvider) where THub : Hub
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private static readonly Lazy<ObjectFactory> _factory = new(() => ActivatorUtilities.CreateFactory(typeof(THub), Type.EmptyTypes));

        public THub Create()
        {
            var hub = _serviceProvider.GetService<THub>() ?? (THub)_factory.Value(_serviceProvider, []);
            return hub;
        }

        public void Release(THub hub)
        {
            hub.Dispose();
        }
    }
}
