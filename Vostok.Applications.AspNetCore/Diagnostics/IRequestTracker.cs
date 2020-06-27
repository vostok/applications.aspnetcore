using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Vostok.Applications.AspNetCore.Models;

namespace Vostok.Applications.AspNetCore.Diagnostics
{
    [PublicAPI]
    public interface IRequestTracker
    {
        [NotNull]
        IDisposable Track([NotNull] HttpContext context, [NotNull] IRequestInfo info);
    }
}
