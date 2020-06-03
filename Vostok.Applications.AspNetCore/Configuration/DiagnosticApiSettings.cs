using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="DiagnosticApiMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class DiagnosticApiSettings
    {
        [NotNull]
        public string PathPrefix = "/_diagnostic";
    }
}
