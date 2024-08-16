namespace LogicalServer.Core
{
    public interface IHubActivator<THub> where THub : Hub
    {
        THub Create();
        void Release(THub hub);
    }
}
