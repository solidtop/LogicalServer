namespace LS.Core
{
    public interface IClientProxy
    {
        Task SendAsync(string methodName, object?[] args, CancellationToken cancellationToken = default);
    }
}
