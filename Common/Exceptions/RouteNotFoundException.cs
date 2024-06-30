namespace LogicalServer.Common.Exceptions
{
    public class RouteNotFoundException(string route) : NotFoundException($"Route not found: {route}")
    {
    }
}
