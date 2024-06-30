namespace LogicalServer.Common.Exceptions
{
    public class MethodNotFoundException(string methodName) : NotFoundException($"Method not found: {methodName}")
    {
    }
}
