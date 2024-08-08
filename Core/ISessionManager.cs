namespace LogicalServer.Core
{
    public interface ISessionManager
    {
        Task AddToSessionAsync(string connectionId, string sessionId, CancellationToken cancellationToken = default);
        Task RemoveFromSessionAsync(string connectionId, string sessionId, CancellationToken cancellationToken = default);
    }
}
