namespace Vostok.Applications.AspNetCore.OpenTelemetry;

// note (ponomaryovigor, 05.09.2024): Temporarily copied from legacy OpenTelemetry.SemanticConventions package
// because it was deprecated and unlisted
internal static class SemanticConventions
{
    public const string AttributeHttpRequestContentLength = "http.request.header.content-length";
    public const string AttributeHttpResponseContentLength = "http.response.header.content-length";
    public const string AttributeClientAddress = "client.address";
}