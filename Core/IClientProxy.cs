﻿namespace LogicalServer.Core
{
    public interface IClientProxy
    {
        Task SendCoreAsync(string methodName, object?[] args, CancellationToken cancellationToken = default);
    }
}
