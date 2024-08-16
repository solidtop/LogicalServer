using LogicalServer.Core.Internal;

namespace LogicalServer.Core
{
    public abstract class Hub : IDisposable
    {
        bool _disposed;

        public IHubCallerClients Clients { get; set; } = default!;
        public HubCallerContext Context { get; set; } = default!;
        public ISessionManager Sessions { get; set; } = default!;

        public virtual Task OnConnectedAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task OnDisconnectedAsync(Exception? exception)
        {

            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing) { }

        public void Dispose()
        {
            if (_disposed) return;

            Dispose(true);

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        ~Hub() => Dispose(false);
    }
}
