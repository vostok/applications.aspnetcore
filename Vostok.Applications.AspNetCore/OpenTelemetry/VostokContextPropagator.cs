#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using OpenTelemetry.Context.Propagation;
using Vostok.Tracing;
using Vostok.Tracing.Abstractions;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

/// <summary>
/// <see cref="TextMapPropagator"/> implementation which extracts tracing context from `Context-Globals` header.
/// </summary>
[PublicAPI]
public class VostokContextPropagator : TextMapPropagator
{
    private readonly VostokContextPropagatorSettings settings;
    private readonly TraceContextSerializer traceContextSerializer;
    internal const string ContextGlobalsHeader = "Context-Globals";
    internal const string DistributedGlobalName = "vostok.tracing.context";

    public VostokContextPropagator(VostokContextPropagatorSettings settings = null)
    {
        this.settings = settings ?? new VostokContextPropagatorSettings();
        traceContextSerializer = new TraceContextSerializer();

        Fields = new HashSet<string>
        {
            ContextGlobalsHeader
        };
    }

    public override void Inject<T>(PropagationContext context, T carrier, Action<T, string, string> setter)
    {
    }

    public override PropagationContext Extract<T>(PropagationContext context, T carrier, Func<T, string, IEnumerable<string>> getter)
    {
        if (carrier == null || getter == null)
            return context;

        var globals = getter(carrier, ContextGlobalsHeader)?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(globals))
            return context;

        var traceContext = ExtractTraceContext(globals);
        if (traceContext != null)
        {
            var traceId = ActivityTraceId.CreateFromString(traceContext.TraceId.ToString("N"));
            var spanId = ActivitySpanId.CreateFromString(traceContext.SpanId.ToString("N").AsSpan()[..16]);
            return new PropagationContext(
                new ActivityContext(traceId, spanId, ActivityTraceFlags.None, isRemote: true),
                context.Baggage);
        }

        return context;
    }

    public override ISet<string> Fields { get; }

    [CanBeNull]
    private unsafe TraceContext ExtractTraceContext(string input)
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
                    if (ptr + sizeof(int) + size <= end)
                        return Encoding.UTF8.GetString(ptr + sizeof(int), size);
                    throw new Exception("Wrong sizes.");
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
            settings.ErrorCallback?.Invoke($"Failed to read property names and values from input string '{input}'.", error);
        }

        return null;
    }
}
#endif