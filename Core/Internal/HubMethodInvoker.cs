using System.Reflection;

namespace LS.Core.Internal
{
    internal sealed class HubMethodInvoker<THub> where THub : Hub
    {
        private readonly Dictionary<string, HubMethodDescriptor> _methodCache = [];

        public HubMethodInvoker()
        {
            CacheMethods();
        }

        public void CacheMethods()
        {
            var hubType = typeof(THub);

            foreach (var method in hubType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if ((method.ReturnType == typeof(Task) ||
                    (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
                    && !method.IsVirtual)
                {
                    _methodCache[method.Name] = new HubMethodDescriptor(method);
                }
            }
        }

        public async Task<object?> InvokeAsync(Hub hub, string methodName, object?[] args)
        {
            if (!_methodCache.TryGetValue(methodName, out var method))
            {
                throw new InvalidOperationException($"Method '{methodName}' not found on hub '{typeof(THub).Name}'.");
            }

            args = ChangeArgTypes(method.Parameters, args);
            return await method.Invoker(hub, args);
        }

        private static object?[] ChangeArgTypes(ParameterInfo[] parameters, object?[] args)
        {
            if (args.Length != parameters.Length)
                throw new ArgumentException("The number of arguments does not match the number of parameters");

            return parameters.Select((param, i) => Convert.ChangeType(args[i], param.ParameterType)).ToArray();
        }
    }
}
