using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Builders;
using Vostok.Applications.AspNetCore.Middlewares;

namespace Vostok.Applications.AspNetCore.Configuration
{
    /// <summary>
    /// Represents configuration of <see cref="HttpContextTweakMiddleware"/>.
    /// </summary>
    [PublicAPI]
    public class HttpContextTweakSettings
    {
        /// <summary>
        /// <para>If enabled, splits single large writes to response <see cref="HttpResponse.Body"/> stream into multiple smaller ones.</para>
        /// <para>A large write is anything larger than <see cref="MaxResponseWriteCallSize"/>.</para>
        /// <para>This helps in preventing uncontrolled response buffering when writing from large buffers.</para>
        /// </summary>
        public bool EnableResponseWriteCallSizeLimit { get; set; } = true;

        /// <summary>
        /// Limit value for <see cref="EnableResponseWriteCallSizeLimit"/> option. Default value is 64 KB.
        /// </summary>
        public int MaxResponseWriteCallSize { get; set; } = VostokKestrelBuilder.MaxResponseBufferSize;
    }
}