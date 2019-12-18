using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Vostok.Hosting.AspNetCore.Middlewares
{
    /// <summary>
    /// Configuration of the <see cref="TracingMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class RestoreDistributedContextMiddlewareSettings
    {
        /// <summary>
        /// Additional action that will be executed during distributed context restoring.
        /// </summary>
        [CanBeNull]
        public Action<HttpRequest> AdditionalRestoreDistributedContextAction { get; set; }
    }
}