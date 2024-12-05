using System;
using JetBrains.Annotations;

namespace Vostok.Applications.AspNetCore.OpenTelemetry;

internal static class TraceParentHeaderHelper
{
    // {version}-{trace_id}-{span_id}-{trace_flags}
    private const int TraceParentLengthV0 = 2 + 1 + 32 + 1 + 16 + 1 + 2;

    private const string GuidFormat = "n";

    /// <summary>
    /// <para>Simple "traceparent" header parser to extract TraceId and SpanId for Vostok tracing.</para> 
    /// <para>Works only with 00 version, doesn't check lowercase invariants and doesn't check flags because they don't necessary for Vostok.</para>
    /// <remarks>If there will be new format versions or other problems with this solution, feel free to replace it with text propagators from OpenTelemetry.Api package.</remarks>
    /// </summary>
    public static bool TryParseV0([CanBeNull] string traceParent, out Guid traceId, out Guid spanId)
    {
        traceId = Guid.Empty;
        spanId = Guid.Empty;

        if (traceParent == null || traceParent.Length != TraceParentLengthV0)
            return false;

        if (traceParent[2] != '-' || traceParent[35] != '-' || traceParent[52] != '-')
            return false;

        var version = traceParent[0] == '0' && traceParent[1] == '0' ? 0 : -1;
        if (version != 0)
            return false;

#if NETSTANDARD2_0
        if (!Guid.TryParseExact(traceParent.Substring(3, 32), GuidFormat, out traceId))
#else
        if (!Guid.TryParseExact(traceParent.AsSpan(3, 32), GuidFormat, out traceId))
#endif
            return false;

        var parentId = traceParent.AsSpan(36, 16);
        Span<char> parentSpan = stackalloc char[32];
        parentSpan.Fill('0');
        parentId.CopyTo(parentSpan);
#if NETSTANDARD2_0
        if (!Guid.TryParseExact(parentSpan.ToString(), GuidFormat, out spanId))
#else
        if (!Guid.TryParseExact(parentSpan, GuidFormat, out spanId))
#endif
            return false;

        return traceId != Guid.Empty && spanId != Guid.Empty;
    }
}