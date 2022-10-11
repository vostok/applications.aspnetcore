using System;
using JetBrains.Annotations;
using Vostok.Applications.AspNetCore.Configuration;

namespace Vostok.Applications.AspNetCore.Builders
{
    [PublicAPI]
    public interface IVostokMiddlewaresBuilder
    {
        /// <summary>
        /// Disable <see cref="VostokMiddlewaresBuilder"/> middlewares builder.
        /// </summary>
        void Disable();

        /// <summary>
        /// Disable custom internal middleware from <see cref="Vostok.Applications.AspNetCore.Middlewares"/> namespace.
        /// </summary>
        void Disable<TMiddleware>();

        /// <summary>
        /// Enable custom internal middleware from <see cref="Vostok.Applications.AspNetCore.Middlewares"/> namespace.
        /// </summary>
        void Enable<TMiddleware>();
        
        /// <summary>
        /// Check if custom internal middleware from <see cref="Vostok.Applications.AspNetCore.Middlewares"/> namespace is enabled.
        /// </summary>
        bool IsEnabled<TMiddleware>();

        void InjectPreVostok<TMiddleware>();

        void InjectPreVostok<TMiddleware, TBefore>();

        void Customize(Action<TracingSettings> customization);

        void Customize(Action<LoggingSettings> customization);

        void Customize(Action<PingApiSettings> customization);

        void Customize(Action<DiagnosticApiSettings> customization);

        void Customize(Action<DiagnosticFeaturesSettings> customization);

        void Customize(Action<UnhandledExceptionSettings> customization);

        void Customize(Action<FillRequestInfoSettings> customization);

        void Customize(Action<DistributedContextSettings> customization);

        void Customize(Action<DatacenterAwarenessSettings> customization);

        void Customize(Action<HttpContextTweakSettings> customization);
    }
}