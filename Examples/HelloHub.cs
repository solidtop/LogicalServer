using LogicalServer.Hubs;

namespace LogicalServer.Examples
{
    public class HelloHub(ILogger<HelloHub> logger) : Hub
    {
        private readonly ILogger<HelloHub> _logger = logger;

        public Task Hello(string message)
        {
            _logger.LogInformation(message);
            return Task.CompletedTask;
        }

        public override Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return Task.CompletedTask;
        }
    }
}
