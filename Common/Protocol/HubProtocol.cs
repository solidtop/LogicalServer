using LogicalServer.Core.Internal;
using MessagePack;
using MessagePack.Resolvers;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace LogicalServer.Common.Messaging
{
    public static class HubProtocol
    {
        public static bool TryParseMessage(ref ReadOnlySequence<byte> buffer, IInvocationBinder binder, out HubMessage? message)
        {
            if (!BinaryMessageParser.TryParseMessage(ref buffer, out var payload))
            {
                message = null;
                return false;
            }

            var reader = new MessagePackReader(payload);
            message = ParseMessage(ref reader, binder);

            return message != null;
        }

        private static HubMessage? ParseMessage(ref MessagePackReader reader, IInvocationBinder binder)
        {
            var messageType = (MessageType)reader.ReadInt32();

            return messageType switch
            {
                MessageType.Invocation => CreateInvocationMessage(ref reader, binder),
                MessageType.Close => CreateCloseMessage(ref reader),
                _ => throw new ArgumentException($"Unknown message type: {messageType}"),
            };
        }

        private static HubInvocationMessage CreateInvocationMessage(ref MessagePackReader reader, IInvocationBinder binder)
        {
            var invocationId = ReadString(ref reader, "invocationId");

            if (string.IsNullOrEmpty(invocationId))
            {
                invocationId = null;
            }

            var methodName = ReadString(ref reader, "methodName");

            ThrowIfNullOrEmpty(methodName, "MethodName for HubInvocationMessage");

            object?[]? arguments;

            try
            {
                var parameterTypes = binder.GetParameterTypes(methodName);
                arguments = BindArguments(ref reader, parameterTypes);
            }
            catch
            {
                throw new InvalidOperationException($"Failed to bind arguments to method: {methodName}");
            }

            return new HubInvocationMessage(invocationId, methodName, arguments);
        }

        private static CloseMessage CreateCloseMessage(ref MessagePackReader reader)
        {
            var error = ReadString(ref reader, "error");
            var allowReconnect = ReadBoolean(ref reader, "allowReconnect");

            return new CloseMessage(error, allowReconnect);
        }

        private static object?[] BindArguments(ref MessagePackReader reader, IReadOnlyList<Type> parameterTypes)
        {
            var argumentCount = ReadArrayLength(ref reader, "arguments");

            if (parameterTypes.Count != argumentCount)
            {
                throw new InvalidDataException(
                    $"Invocation provides {argumentCount} argument(s) but method expects {parameterTypes.Count}.");
            }

            try
            {
                var arguments = new object?[argumentCount];

                for (var i = 0; i < argumentCount; i++)
                {
                    arguments[i] = DeserializeObject(ref reader, parameterTypes[i], "argument");
                }

                return arguments;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Error binding arguments. Make sure that the types of the provided values match the types of the hub method being invoked.", ex);
            }
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

        private static bool ReadBoolean(ref MessagePackReader reader, string field)
        {
            try
            {
                return reader.ReadBoolean();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Reading '{field}' as boolean failed.", ex);
            }
        }

        private static long ReadArrayLength(ref MessagePackReader reader, string field)
        {
            try
            {
                return reader.ReadArrayHeader();
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Reading array length for '{field}' failed.", ex);
            }
        }

        private static object? DeserializeObject(ref MessagePackReader reader, Type type, string field)
        {
            try
            {
                return MessagePackSerializer.Deserialize(type, ref reader, ContractlessStandardResolver.Options);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Deserializing object of the `{type.Name}` type for '{field}' failed.", ex);
            }
        }

        private static void ThrowIfNullOrEmpty([NotNull] string? target, string message)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new InvalidDataException($"Null or empty {message}.");
            }
        }

        public static ReadOnlyMemory<byte> GetMessageBytes(HubMessage message)
        {
            var bufferWriter = MemoryBufferWriter.Get();

            try
            {
                var writer = new MessagePackWriter(bufferWriter);
                WriteMessageCore(message, ref writer);

                var dataLength = bufferWriter.Length;
                var prefixLength = BinaryMessageFormatter.LengthPrefixLength(dataLength);

                var array = new byte[dataLength + prefixLength];
                var span = array.AsSpan();

                var written = BinaryMessageFormatter.WriteLengthPrefix(dataLength, span);
                bufferWriter.CopyTo(span.Slice(prefixLength));

                return array;
            }
            finally
            {
                MemoryBufferWriter.Return(bufferWriter);
            }
        }

        public static void WriteMessage(HubMessage message, IBufferWriter<byte> output)
        {
            var bufferWriter = MemoryBufferWriter.Get();

            try
            {
                var writer = new MessagePackWriter(bufferWriter);
                WriteMessageCore(message, ref writer);
                BinaryMessageFormatter.WriteLengthPrefix(bufferWriter.Length, output);
                bufferWriter.CopyTo(output);
            }
            finally
            {
                MemoryBufferWriter.Return(bufferWriter);
            }
        }

        private static void WriteMessageCore(HubMessage message, ref MessagePackWriter writer)
        {
            switch (message)
            {
                case HandshakeResponseMessage handshakeMessage:
                    WriteHandshakeMessage(handshakeMessage, ref writer);
                    break;
                case InvocationMessage invocationMessage:
                    WriteInvocationMessage(invocationMessage, ref writer);
                    break;
                case CompletionMessage completionMessage:
                    WriteCompletionMessage(completionMessage, ref writer);
                    break;
                case CloseMessage closeMessage:
                    WriteCloseMessage(closeMessage, ref writer);
                    break;
                default:
                    throw new InvalidDataException($"Unexpected message type: {message.GetType().Name}");
            }

            writer.Flush();
        }

        private static void WriteHandshakeMessage(HandshakeResponseMessage message, ref MessagePackWriter writer)
        {
            writer.Write((byte)message.Type);

            if (message.Error is null)
            {
                writer.WriteNil();
            }
            else
            {
                writer.Write(message.Error);
            }
        }

        private static void WriteInvocationMessage(InvocationMessage message, ref MessagePackWriter writer)
        {
            writer.Write((byte)message.Type);
            writer.Write(message.MethodName);

            if (message.Arguments is null)
            {
                writer.WriteArrayHeader(0);
            }
            else
            {
                writer.WriteArrayHeader(message.Arguments.Length);

                foreach (var arg in message.Arguments)
                {
                    WriteArgument(arg, ref writer);
                }
            }
        }

        private static void WriteArgument(object? argument, ref MessagePackWriter writer)
        {
            if (argument is null)
            {
                writer.WriteNil();
            }
            else
            {
                Serialize(ref writer, argument.GetType(), argument);
            }
        }

        private static void WriteCompletionMessage(CompletionMessage message, ref MessagePackWriter writer)
        {
            writer.Write((byte)message.Type);
            writer.Write(message.InvocationId);

            if (message.Error is null)
            {
                writer.WriteNil();
            }
            else
            {
                writer.Write(message.Error);
            }

            if (message.Result is null)
            {
                writer.WriteNil();
            }
            else
            {
                WriteArgument(message.Result, ref writer);
            }

            writer.Write(message.HasResult);
        }

        private static void WriteCloseMessage(CloseMessage message, ref MessagePackWriter writer)
        {
            writer.Write((byte)message.Type);
            writer.Write(message.Error);
            writer.Write(message.AllowReconnect);
        }

        private static void Serialize(ref MessagePackWriter writer, Type type, object value)
        {
            MessagePackSerializer.Serialize(type, ref writer, value, ContractlessStandardResolver.Options);
        }
    }
}
