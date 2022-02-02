using Vostok.Hosting.Abstractions.Diagnostics;
using Vostok.Throttling;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class ThrottlingInfoProvider : IDiagnosticInfoProvider
    {
        private readonly ThrottlingProvider throttlingProvider;

        public ThrottlingInfoProvider(ThrottlingProvider throttlingProvider)
            => this.throttlingProvider = throttlingProvider;

        public object Query()
            => throttlingProvider.CurrentInfo;
    }
}