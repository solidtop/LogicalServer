using LogicalServer.Core;
using LS.Core;

namespace LogicalServer.Examples
{
    public class HelloHub(ILogger<HelloHub> logger) : Hub
    {
        private readonly ILogger<HelloHub> _logger = logger;

        public async Task Hello(string message)
        {
            _logger.LogInformation(message);

            // All available hub methods
            await Clients.Caller.SendAsync("Hello", message);
            await Clients.Others.SendAsync("Hello", message);
            await Clients.All.SendAsync("Hello", message);
            await Clients.AllExcept([Context.ConnectionId]).SendAsync("Hello", message);
            await Clients.Client(Context.ConnectionId).SendAsync("Hello", message);
            await Clients.Session("ExampleSession1").SendAsync("Hello", message);
            await Clients.SessionExcept("ExampleSession1", [Context.ConnectionId]).SendAsync("Hello", message);
            await Clients.Sessions(["ExampleSession1", "ExampleSession2"]).SendAsync("Hello", message);
        }

        public override Task OnConnectedAsync()
        {
            Sessions.AddToSessionAsync(Context.ConnectionId, "ExampleSession1");
            Sessions.AddToSessionAsync(Context.ConnectionId, "ExampleSession2");

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Sessions.RemoveFromSessionAsync(Context.ConnectionId, "ExampleSession1");
            Sessions.RemoveFromSessionAsync(Context.ConnectionId, "ExampleSession2");

            return base.OnDisconnectedAsync(exception);
        }
    }
}
