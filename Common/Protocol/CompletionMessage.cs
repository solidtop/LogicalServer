﻿namespace LogicalServer.Common.Messaging
{
    public class CompletionMessage : HubMessage
    {
        public string InvocationId { get; }
        public string? Error { get; }
        public object? Result { get; }
        public bool HasResult { get; }

        public CompletionMessage(string invocationId, string? error, object? result, bool hasResult)
        {
            InvocationId = invocationId;
            Error = error;
            Result = result;
            HasResult = hasResult;
            Type = MessageType.Completion;
        }

        public static CompletionMessage WithError(string invocationId, string? error)
            => new(invocationId, error, result: null, hasResult: false);

        public static CompletionMessage WithResult(string invocationId, object? result)
            => new(invocationId, error: null, result, hasResult: true);
    }
}