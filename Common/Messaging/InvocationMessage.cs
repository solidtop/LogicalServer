namespace LogicalServer.Common.Messaging
{
    public class InvocationMessage : HubMessage
    {
        public string MethodName { get; }
        public object?[] Arguments { get; }

        public InvocationMessage(string methodName, object?[] arguments)
        {
            MethodName = methodName;
            Arguments = arguments;
            Type = MessageType.Invocation;
        }
    }
}
