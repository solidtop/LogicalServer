
using LogicalServer.Common;

namespace LogicalServer.Core.Internal
{
    internal sealed class InvocationBinder<THub>(IHubDispatcher<THub> dispatcher) : IInvocationBinder where THub : Hub
    {
        private readonly IHubDispatcher<THub> _dispatcher = dispatcher;

        public IReadOnlyList<Type> GetParameterTypes(string methodName)
        {
            return _dispatcher.GetParameterTypes(methodName);
        }
    }
}
