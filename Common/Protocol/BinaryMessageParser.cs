using System.Buffers;

namespace LogicalServer.Core.Internal
{
    internal static class BinaryMessageParser
    {
        private const int MaxLengthPrefixSize = 5;

        public static bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> payload)
        {
            if (buffer.IsEmpty)
            {
                payload = default;
                return false;
            }

            var length = 0U;
            var numBytes = 0;

            var lengthPrefixBuffer = buffer.Slice(0, Math.Min(MaxLengthPrefixSize, buffer.Length));
            var span = GetSpan(lengthPrefixBuffer);

            byte byteRead;
            do
            {
                byteRead = span[numBytes];
                length = length | (((uint)(byteRead & 0x7f)) << (numBytes * 7));
                numBytes++;
            }
            while (numBytes < lengthPrefixBuffer.Length && ((byteRead & 0x80) != 0));

            // size bytes are missing
            if ((byteRead & 0x80) != 0 && (numBytes < MaxLengthPrefixSize))
            {
                payload = default;
                return false;
            }

            if ((byteRead & 0x80) != 0 || (numBytes == MaxLengthPrefixSize && byteRead > 7))
            {
                throw new FormatException("Messages over 2GB in size are not supported.");
            }

            // We don't have enough data
            if (buffer.Length < length + numBytes)
            {
                payload = default;
                return false;
            }

            // Get the payload
            payload = buffer.Slice(numBytes, (int)length);

            // Skip the payload
            buffer = buffer.Slice(numBytes + (int)length);

            return true;
        }

        private static ReadOnlySpan<byte> GetSpan(in ReadOnlySequence<byte> lengthPrefixBuffer)
        {
            if (lengthPrefixBuffer.IsSingleSegment)
            {
                return lengthPrefixBuffer.FirstSpan;
            }

            // Should be rare
            return lengthPrefixBuffer.ToArray();
        }
    }
}
