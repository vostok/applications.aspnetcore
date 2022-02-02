using System.Linq;
using Vostok.Hosting.Abstractions.Diagnostics;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class CurrentRequestsInfoProvider : IDiagnosticInfoProvider
    {
        private readonly RequestTracker tracker;

        public CurrentRequestsInfoProvider(RequestTracker tracker)
            => this.tracker = tracker;

        public object Query()
        {
            var requests = tracker.CurrentItems
                .Select(item => new {item.Path, item.Info.ElapsedTime})
                .OrderByDescending(item => item.ElapsedTime)
                .ToArray();

            return new
            {
                Count = requests.Length,
                Requests = requests
            };
        }
    }
}