using LogicalServer.Core.Internal;
using MessagePack;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace LogicalServer.Common.Messaging
{
    public static class HandshakeParser
    {
        public static bool TryParseRequestMessage(ref ReadOnlySequence<byte> buffer, out HandshakeRequestMessage? message)
        {
            if (!BinaryMessageParser.TryParseMessage(ref buffer, out var payload))
            {
                message = null;
                return false;
            }

            var reader = new MessagePackReader(payload);
            message = ParseMessage(ref reader);

            return message != null;
        }

        private static HandshakeRequestMessage? ParseMessage(ref MessagePackReader reader)
        {
            var messageType = (MessageType)reader.ReadInt32();

            if (messageType != MessageType.Handshake)
            {
                throw new InvalidDataException($"Invalid message type for handshake request");
            }

            var hubName = ReadString(ref reader, "hubName");

            ThrowIfNullOrEmpty(hubName, "HubName for HandshakeRequestMessage");

            return new HandshakeRequestMessage(hubName);
        }

        private static string? ReadString(ref MessagePackReader reader, string field)
        {
            try
            {
                return reader.ReadString();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Reading '{field}' as String failed.", ex);
            }
        }

        private static void ThrowIfNullOrEmpty([NotNull] string? target, string message)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new InvalidDataException($"Null or empty {message}.");
            }
        }
    }
}
