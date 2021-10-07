## 0.2.14 (07-10-2021):

Added `WarmupServicesAsync` method.

## 0.2.13 (27-07-2021):

Public ServiceCollectionExtensions.

## 0.2.12 (26-06-2021):

Mitigated https://github.com/vostok/applications.aspnetcore/issues/47

## 0.2.11 (06-05-2021):

Do not send ping request if PingApiMiddleware disabled.

## 0.2.10 (14-04-2021):

Added graceful shutdown of VostokHost when Microsoft AspNetCore host has stopped.

## 0.2.9 (03-02-2021):

Logging improvements.

## 0.2.8 (13-01-2021):

Added a scoped registration of IRequestInfo to the DI container.

## 0.2.7 (23-12-2020):

Added a net5.0 target.

## 0.2.6 (03-12-2020):

- Added `DoDisposeAsync` virtual method.

## 0.2.5 (18-11-2020):

- Fixed `_diagnostic` relative path bug.
- Added middlewares warmup during initialization to reduce first request time.

## 0.2.4 (30-10-2020):

Register required configuration types as `Func<TSettings>`.

## 0.2.3 (23-10-2020):

VostokAspNetCoreApplication, VostokNetCoreApplication: made InitializeAsync method virtual.

## 0.2.2 (20-07-2020):

Implemented a limit on the size of single WriteAsync call to response body to prevent excessive response buffering and inefficiency of throttling middleware.

## 0.2.1 (14-07-2020):

ThrottlingMiddleware no longer passes requests aborted during throttlings (this may occur if client does not reveal its true timeout or closes connection unexpectedly).

## 0.2.0 (27-06-2020):

New features:
- Diagnostic API middleware
- Diagnostic info providers (throttling, current requests)
- Throttling-based health check
- 2-way integration with Asp.NET Core health checks (only on .NET Core)
- Integration of new health checks with ping API
- Conversion between IVostokApplication and IHostedService
- AddAspNetCore and AddNetCore extensions for ICompositeApplicationBuilder

Minor tweaks:
- LoggingSettings: do not log response completion events by default.

## 0.1.18 (26-06-2020):

Enabled SourceLink.

## 0.1.16 (26-06-2020):

- Ignore all Microsoft log scopes by default.
- Use Warning level for MS logs when user doesn't specify it explicitly.

## 0.1.14 (20-05-2020):

- UnhandledExceptionMiddleware no longer swallows exceptions that happen during response body streaming (that could prevent Kestrel from flushing its output buffers).
- UnhandledExceptionMiddleware now clears any custom headers the app might have set when responding with HTTP 500.
- TracingMiddleware now formats trace ids in response header without dashes ("N" format).
- UsePathBase middleware is now registered before Vostok middlewares --> ping API works with base URL prefix.
- Throttling provider is now registered in DI container even if ThrottlingMiddleware is disabled in case user adds it back manually in a different position.

## 0.1.13 (01-05-2020):

- User application classes can now override `DoDispose` method to perform cleanup.
- Added a couple of functional tests for middlewares.

## 0.1.12 (30-04-2020):

Fill headers in ping api middleware.

## 0.1.11 (01-04-2020):

https://github.com/vostok/applications.aspnetcore/issues/19

## 0.1.10 (24-03-2020):

Do not explicitly disallow synchronous I/O in Kestrel, rely on Asp.NET Core defaults.

## 0.1.9 (19-03-2020):

Use Microsoft constant from `vostok.logging.microsoft` module.

## 0.1.8 (13-03-2020):

Major changes in this release:
* Multitargeting. In addition to `netcoreapp3.1` we now also target `netstandard2.0` and Asp.NET Core 2.1 (see https://github.com/vostok/applications.aspnetcore/issues/8). It's also reflected in Cement `module.yaml` as two new module configurations: `v3_1` (default) and `v2_1`. This allows to use the library in .NET Framework applications.
* Built-in middlewares no longer implement `IMiddleware` interface.
* Built-in middlewares are no longer instantiated manually.
* Built-in middlewares now use options pattern to receive configuration from DI container.
* Built-in middleware classes are all public now.
* Built-in middlewares can now be disabled, both entirely and selectively.
* Built-in middlewares can now be added with public `IApplicationBuilder` extensions.
* Significant internal refactoring necessitated by multitargeting approach.

Minor changes:
* (**Breaking change!**) `ThrottlingMetricsOptions` have been moved from `ThrottlingSettings` to an `IVostokThrottlingBuilder` property.
* `UnhandledExceptionMiddleware` now has its own settings class.
* All middlewares now enrich their log instances with source context.
* `LoggingMiddleware`, `TracingMiddleware` and `ThrottlingMiddleware` now tolerate absence of the `FillRequestInfoMiddleware`.
* `LoggingMiddleware` now also logs response completion time, which includes the time it takes to send data to the client (save for small buffered writes).
* Added xml-docs for `IVostokAspNetCoreApplicationBuilder` methods.
* Added configuration of generic host shutdown timeout based on Vostok environment.

## 0.1.7 (07-03-2020):

* https://github.com/vostok/applications.aspnetcore/issues/9
* https://github.com/vostok/applications.aspnetcore/issues/10

## 0.1.6 (03-03-2020):

* https://github.com/vostok/applications.aspnetcore/issues/6
* https://github.com/vostok/applications.aspnetcore/issues/7

## 0.1.5 (02-03-2020):

Added `VostokNetCoreApplication` instead of `DisableWebHost`.

## 0.1.4 (03-02-2020):

Added `UseCustomPropertyQuota` extension to configure throttling by custom property.

## 0.1.3 (30-01-2020):

Extract commit hash from calling assembly instead of entry assembly.

## 0.1.2 (30-01-2020):

Added `VostokAspNetCoreApplication` without `Startup`.

## 0.1.1 (28-01-2020):

* Added an option to disable WebHost.
* Added arbitrary customization of generic host.
* Added an extension to register IHostedServices.

## 0.1.0 (18-01-2020): 

Initial prerelease.
