using System.Buffers;

namespace LogicalServer.Core.Internal
{
    internal static class TextMessageParser
    {
        private const byte recordSeparator = 0x1e;

        public static bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            if (buffer.IsSingleSegment)
            {
                var span = buffer.FirstSpan;
                int index = span.IndexOf(recordSeparator);

                if (index == -1)
                {
                    payload = default;
                    return false;
                }

                payload = buffer.Slice(0, index);

                buffer = buffer.Slice(index + 1);

                return true;
            }
            else
            {
                return TryParseMessageMultiSegment(ref buffer, out payload);
            }
        }

        private static bool TryParseMessageMultiSegment(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            var position = buffer.PositionOf(recordSeparator);

            if (position == null)
            {
                payload = default;
                return false;
            }

            payload = buffer.Slice(0, position.Value);

            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

            return true;
        }
    }
}
