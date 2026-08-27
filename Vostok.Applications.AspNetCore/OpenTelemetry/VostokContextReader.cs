#if NET6_0_OR_GREATER
using System;
using System.Buffers.Binary;
using System.Text;
using JetBrains.Annotations;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

internal class VostokContextReader
{
    internal const string DistributedGlobalName = "vostok.tracing.context";

    private readonly TraceContextSerializer traceContextSerializer;
    private readonly Action<string, Exception> errorCallback;

    public VostokContextReader(Action<string, Exception> errorCallback)
    {
        this.errorCallback = errorCallback;
        traceContextSerializer = new TraceContextSerializer();
    }
    
    [CanBeNull]
    public TraceContext Read(string input)
    {
        try
        {
            ReadOnlySpan<byte> remainingBytes = Convert.FromBase64String(input);

            while (remainingBytes.Length > 0)
            {
                remainingBytes = ReadString(remainingBytes, out var key, DistributedGlobalName.Length);
                remainingBytes = ReadString(remainingBytes, out var value);

                if (key == DistributedGlobalName)
                    return traceContextSerializer.Deserialize(value);
            }
        }
        catch (Exception error)
        {
            errorCallback?.Invoke($"Failed to read property names and values from input string '{input}'.", error);
        }

        return null;
    }

    private static ReadOnlySpan<byte> ReadString(ReadOnlySpan<byte> bytes, out string value, int? expectedSize = null)
    {
        const int sizeOfInt = sizeof(int);

        if (bytes.Length < sizeOfInt)
            throw new InvalidOperationException("Buffer ended unexpectedly while reading string length.");

        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes);
        bytes = bytes[sizeOfInt..];

        if (size < 0)
            throw new InvalidOperationException("Negative string size.");
        if (size > bytes.Length)
            throw new InvalidOperationException("String size exceeds remaining buffer.");

        var stringBytes = bytes[..size];
        value = !expectedSize.HasValue || size == expectedSize.Value
            ? Encoding.UTF8.GetString(stringBytes)
            : string.Empty;

        return bytes[size..];
    }
}
#endif