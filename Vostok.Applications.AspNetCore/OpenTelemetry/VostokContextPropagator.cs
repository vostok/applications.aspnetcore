﻿#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using OpenTelemetry.Context.Propagation;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

/// <summary>
/// <see cref="TextMapPropagator"/> implementation which extracts tracing context from `Context-Globals` header.
/// </summary>
[PublicAPI]
public class VostokContextPropagator : TextMapPropagator
{
    private readonly VostokContextPropagatorSettings settings;
    private readonly VostokContextReader vostokContextReader;
    internal const string ContextGlobalsHeader = "Context-Globals";

    public VostokContextPropagator(VostokContextPropagatorSettings settings = null)
    {
        this.settings = settings ?? new VostokContextPropagatorSettings();

        vostokContextReader = new VostokContextReader(this.settings.ErrorCallback);

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

        var traceContext = vostokContextReader.Read(globals);
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
}
#endif