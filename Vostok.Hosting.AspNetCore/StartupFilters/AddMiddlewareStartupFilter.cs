using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Vostok.Hosting.AspNetCore.StartupFilters
{
    internal class AddMiddlewareStartupFilter<T> : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseMiddleware<T>();
                next(app);
            };
        }
    }
}