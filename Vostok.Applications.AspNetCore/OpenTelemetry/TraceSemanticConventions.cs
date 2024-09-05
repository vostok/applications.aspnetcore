// ReSharper disable once CheckNamespace
namespace OpenTelemetry.Trace;

// note (ponomaryovigor, 05.09.2024): Temporarily copied from legacy OpenTelemetry.SemanticConventions package
// because it was deprecated and unlisted
internal static class TraceSemanticConventions
{
    public const string AttributeHttpRequestContentLength = "http.request_content_length";
    public const string AttributeHttpResponseContentLength = "http.response_content_length";
    public const string AttributeHttpClientIp = "http.client_ip";
}