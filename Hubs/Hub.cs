using LogicalServer.Session;
using System.Reflection;

namespace LogicalServer.Hubs
{
    public abstract class Hub
    {
        public required string Route { get; set; }

        public HubClients Clients { get; set; } = default!;

        public HubContext Context { get; set; } = default!;

        public SessionManager Sessions { get; set; } = default!;

        public async Task Invoke(string methodName, object?[] data)
        {
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
            var method = GetType().GetMethod(methodName, bindingAttr) ?? throw new MissingMethodException($"Method {methodName} not found");
            var parameters = method.GetParameters();

            var args = parameters.Select((param, i) => Convert.ChangeType(data[i], param.ParameterType)).ToArray();
            var result = method.Invoke(this, args);

            if (result is Task taskResult)
            {
                await taskResult;
            }
            else
            {
                await Task.CompletedTask;
            }
        }

        public virtual Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDisconnectedAsync(Exception? exception)
        {
            return Task.CompletedTask;
        }
    }
}
