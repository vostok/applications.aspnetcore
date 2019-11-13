using System;
using Vostok.Commons.Environment;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.AspNetCore.Middlewares;
using Vostok.Hosting.AspNetCore.Setup;
// ReSharper disable ParameterHidesMember

namespace Vostok.Hosting.AspNetCore.Builders
{
    internal class PingApiMiddlewareBuilder : IVostokPingApiMiddlewareBuilder
    {
        private volatile Func<string> statusProvider;
        private volatile Func<string> commitHashProvider;
        private volatile bool disabled;

        public PingApiMiddlewareBuilder()
        {
            statusProvider = () => "Ok";
            commitHashProvider = AssemblyCommitHashExtractor.ExtractFromEntryAssembly;
        }

        public IVostokPingApiMiddlewareBuilder Enable()
        {
            disabled = false;
            return this;
        }

        public IVostokPingApiMiddlewareBuilder Disable()
        {
            disabled = true;
            return this;
        }

        public IVostokPingApiMiddlewareBuilder SetStatusProvider(Func<string> statusProvider)
        {
            this.statusProvider = statusProvider ?? throw new ArgumentNullException(nameof(statusProvider));
            return this;
        }

        public IVostokPingApiMiddlewareBuilder SetCommitHashProvider(Func<string> commitHashProvider)
        {
            this.commitHashProvider = commitHashProvider ?? throw new ArgumentNullException(nameof(commitHashProvider));
            return this;
        }

        public PingApiMiddleware Build(IVostokHostingEnvironment environment)
        {
            if (disabled)
                return null;

            var settings = new PingApiMiddlewareSettings(statusProvider, commitHashProvider);

            return new PingApiMiddleware(settings);
        }
    }
}