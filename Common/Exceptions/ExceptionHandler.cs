using LogicalServer.Hubs;
using System.Net.Sockets;
using System.Text;

namespace LogicalServer.Common.Exceptions
{
    public class ExceptionHandler(ILogger<ExceptionHandler> logger) : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger = logger;

        public async Task InvokeAsync(NetworkStream stream, Func<Task> next)
        {
            try
            {
                await next.Invoke();
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(stream, ex);
            }
        }

        public async Task HandleExceptionAsync(NetworkStream stream, Exception exception)
        {
            (string error, string message, DateTime timestamp) = exception switch
            {
                NotFoundException notFound => ("NOT_FOUND", notFound.Message, DateTime.Now),
                _ => default
            };

            if (error == default)
            {
                return;
            }

            _logger.LogInformation(exception.Message);

            var hubError = new HubError(error, message, timestamp);
            var errorStr = HubMessageParser.Parse(hubError);
            var buffer = Encoding.UTF8.GetBytes(errorStr);
            await stream.WriteAsync(buffer);

            return;
        }
    }
}

