namespace LogicalServer.Hubs
{
    public record HubError(
        string Error,
        string Message,
        DateTime Timestamp
        )
    {
    }
}
