using LogicalServer.Common.Exceptions;

namespace LS.Core.Internal
{
    internal static class ErrorMessageHelper
    {
        internal static string BuildErrorMessage(string message, Exception exception)
        {
            if (exception is HubException)
            {
                return $"{message} {exception.GetType().Name}: {exception.Message}";
            }

            return message;
        }
    }
}
