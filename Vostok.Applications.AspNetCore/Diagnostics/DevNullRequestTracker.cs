using System;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Models;
using Vostok.Commons.Helpers.Disposable;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    internal class DevNullRequestTracker : IRequestTracker
    {
        private readonly IDisposable result = new ActionDisposable(() => {});

        public IDisposable Track(HttpContext context, IRequestInfo info) => result;
    }
}
