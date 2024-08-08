namespace LS
{
    public class ServerHostedService(Server server) : IHostedService
    {
        private readonly Server _server = server;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            string ipAdress = "127.0.0.1";
            int port = 8000;

            _server.Start(ipAdress, port);

            await Task.Run(() => _server.AcceptClientsAsync(_cancellationTokenSource.Token), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.CompletedTask;
        }
    }
}
