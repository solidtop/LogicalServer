using System.Buffers;

namespace LogicalServer.Core.Internal
{
    internal static class BinaryMessageFormatter
    {
        public static void WriteLengthPrefix(long length, IBufferWriter<byte> output)
        {
            Span<byte> lenBuffer = stackalloc byte[5];

            var lenNumBytes = WriteLengthPrefix(length, lenBuffer);

            output.Write(lenBuffer[0..lenNumBytes]);
        }

        public static int WriteLengthPrefix(long length, Span<byte> output)
        {
            var lenNumBytes = 0;

            do
            {
                ref var current = ref output[lenNumBytes];
                current = (byte)(length & 0x7f);
                length >>= 7;

                if (length > 0)
                {
                    current |= 0x80;
                }

                lenNumBytes++;
            }
            while (length > 0);

            return lenNumBytes;
        }

        public static int LengthPrefixLength(long length)
        {
            var lenNumBytes = 0;
            do
            {
                length >>= 7;
                lenNumBytes++;
            }
            while (length > 0);

            return lenNumBytes;
        }
    }
}