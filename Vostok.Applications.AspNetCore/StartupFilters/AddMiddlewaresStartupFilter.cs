using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Vostok.Applications.AspNetCore.StartupFilters
{
    internal class AddMiddlewaresStartupFilter : IStartupFilter
    {
        private readonly IReadOnlyList<Type> middlewaresTypes;

        public AddMiddlewaresStartupFilter(params Type[] middlewaresTypes)
            => this.middlewaresTypes = middlewaresTypes;

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                foreach (var type in middlewaresTypes)
                    app.UseMiddleware(type);

                next(app);
            };
        }
    }
}