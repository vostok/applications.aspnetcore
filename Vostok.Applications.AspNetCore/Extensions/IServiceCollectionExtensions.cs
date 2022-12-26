using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vostok.Applications.AspNetCore.Helpers;
using Vostok.Hosting.Abstractions;

namespace Vostok.Applications.AspNetCore.Extensions;

[PublicAPI]
public static class IServiceCollectionExtensions
{
    /// <inheritdoc cref="AddHostedServiceFromApplication{TApplication}(Microsoft.Extensions.DependencyInjection.IServiceCollection, TApplication)"/>
    public static IServiceCollection AddHostedServiceFromApplication<TApplication>(this IServiceCollection services)
        where TApplication : class, IVostokApplication
    {
        services.AddSingleton<TApplication>();
        services.AddHostedService<VostokApplicationHostedService<TApplication>>();
        return services;
    }

    /// <summary>
    /// <para>Adds given <paramref name="application"/> as <see cref="IHostedService"/>.</para>
    /// <para><see cref="IVostokApplication.InitializeAsync"/> and <see cref="IVostokApplication.RunAsync"/> are called during <see cref="IHostedService.StartAsync"/> phase.</para>
    /// <para>Waits to completion of <see cref="IVostokApplication.RunAsync"/> during <see cref="IHostedService.StopAsync"/> phase.</para>
    /// <para>Also consider using <see cref="AddBackgroundServiceFromApplication{TApplication}(Microsoft.Extensions.DependencyInjection.IServiceCollection, TApplication)"/> instead.</para>
    /// </summary>
    public static IServiceCollection AddHostedServiceFromApplication<TApplication>(this IServiceCollection services, TApplication application)
        where TApplication : class, IVostokApplication
    {
        services.AddSingleton(_ => application);
        services.AddHostedService<VostokApplicationHostedService<TApplication>>();
        return services;
    }
        
    /// <inheritdoc cref="AddBackgroundServiceFromApplication{TApplication}(Microsoft.Extensions.DependencyInjection.IServiceCollection, TApplication)"/>
    public static IServiceCollection AddBackgroundServiceFromApplication<TApplication>(this IServiceCollection services)
        where TApplication : class, IVostokApplication
    {
        services.AddSingleton<TApplication>();
        services.AddHostedService<VostokApplicationBackgroundService<TApplication>>();
        return services;
    }

    /// <summary>
    /// <para>Adds given <paramref name="application"/> as <see cref="BackgroundService"/>.</para>
    /// <para>Doesn't wait completion of <see cref="IVostokApplication.InitializeAsync"/> or <see cref="IVostokApplication.RunAsync"/>.</para>
    /// <para>Also consider using <see cref="AddHostedServiceFromApplication{TApplication}(Microsoft.Extensions.DependencyInjection.IServiceCollection, TApplication)"/> instead.</para>
    /// </summary>
    public static IServiceCollection AddBackgroundServiceFromApplication<TApplication>(this IServiceCollection services, TApplication application)
        where TApplication : class, IVostokApplication
    {
        services.AddSingleton(_ => application);
        services.AddHostedService<VostokApplicationBackgroundService<TApplication>>();
        return services;
    }
}