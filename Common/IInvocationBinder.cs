namespace LogicalServer.Common
{
    public interface IInvocationBinder
    {
        IReadOnlyList<Type> GetParameterTypes(string methodName);
    }
}
