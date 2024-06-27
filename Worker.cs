using Microsoft.Extensions.Options;

namespace LogicalServer
{
    public class Worker(ILogger<Worker> logger, Server server, IOptions<ServerOptions> options) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string ip = options.Value.IpAdress;
            int port = options.Value.Port;

            server.Listen(ip, port, () => logger.LogInformation("Listening on port {port}...", port));
            await server.AcceptClientsAsync(stoppingToken);
        }
    }
}
