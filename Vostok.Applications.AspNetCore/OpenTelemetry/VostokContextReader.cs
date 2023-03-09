#if NET6_0_OR_GREATER
using System;
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
    public unsafe TraceContext Read(string input)
    {
        try
        {
            var bytes = Convert.FromBase64String(input);

            fixed (byte* begin = bytes)
            {
                var end = begin + bytes.Length;

                var ptr = begin;

                string ReadString(out int size, int? expectedSize = null)
                {
                    size = *(int*)ptr;
                    if (size < 0)
                        throw new Exception("Negative size.");
                    if (expectedSize.HasValue && size != expectedSize)
                        return string.Empty;
                    if (ptr + sizeof(int) + size > end)
                        throw new Exception("Wrong sizes.");
                    return Encoding.UTF8.GetString(ptr + sizeof(int), size);
                }

                while (ptr < end)
                {
                    var key = ReadString(out var size, DistributedGlobalName.Length);
                    if (key == DistributedGlobalName)
                    {
                        ptr += sizeof(int) + size;
                        var value = ReadString(out _);
                        return traceContextSerializer.Deserialize(value);
                    }

                    ptr += sizeof(int) + size;
                    ReadString(out size, -1);
                    ptr += sizeof(int) + size;
                }
            }
        }
        catch (Exception error)
        {
            errorCallback?.Invoke($"Failed to read property names and values from input string '{input}'.", error);
        }

        return null;
    }
}
#endif