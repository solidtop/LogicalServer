using System.IO.Pipelines;

namespace LogicalServer.Common.Messaging
{
    public class HubTransport(PipeReader input, PipeWriter output) : IDuplexPipe
    {
        public PipeReader Input => input;

        public PipeWriter Output => output;
    }
}
