namespace LogicalServer.Hubs
{
    public record HubMessage(string Route, string MethodName, object?[] Data)
    {
    }
}
