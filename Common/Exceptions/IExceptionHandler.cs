using System.Net.Sockets;

namespace LogicalServer.Common.Exceptions
{
    public interface IExceptionHandler
    {
        Task InvokeAsync(NetworkStream stream, Func<Task> next);
        Task HandleExceptionAsync(NetworkStream stream, Exception exception);
    }
}
