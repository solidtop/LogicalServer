namespace LogicalServer.Common.Messaging
{
    public class HubInvocationMessage : HubMessage
    {
        public string InvocationId { get; }
        public string MethodName { get; }
        public object?[] Arguments { get; }

        public HubInvocationMessage(string invocationId, string methodName, object?[] arguments)
        {
            InvocationId = invocationId;
            MethodName = methodName;
            Arguments = arguments;
            Type = MessageType.Invocation;
        }
    }
}
