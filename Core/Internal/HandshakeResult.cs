namespace LogicalServer.Core.Internal
{
    internal class HandshakeResult(bool success, IConnectionHandler? handler)
    {
        public bool IsSuccess { get; } = success;
        public IConnectionHandler? Handler { get; } = handler;

        public static HandshakeResult Success(IConnectionHandler handler) => new(true, handler);
        public static HandshakeResult Fail() => new(false, null);
    }
}
