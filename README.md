# Vostok.Hosting.AspNetCore

[![Build status](https://ci.appveyor.com/api/projects/status/github/vostok/hosting.aspnetcore?svg=true&branch=master)](https://ci.appveyor.com/project/vostok/hosting.aspnetcore/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Vostok.Hosting.AspNetCore.svg)](https://www.nuget.org/packages/Vostok.Hosting.AspNetCore)

AspNetCore vostok application template.


**Build guide**: https://github.com/vostok/devtools/blob/master/library-dev-conventions/how-to-build-a-library.md

**User documentation**: not written yet.

## How to use

#### Create new project
    
    dotnet new webapi --framework netcoreapp3.1 -o MyProject

#### Add reference to `vostok.hosting.aspnetcore` module
    
    cm ref add vostok.hosting.aspnetcore MyProject/MyProject.csproj

#### Write vostok AspNetCore application

    public class MyAspNetCoreApplication : VostokAspNetCoreApplication
    {
        public override void Setup(IVostokAspNetCoreApplicationBuilder builder, IVostokHostingEnvironment environment)
        {
            builder.SetupWebHost(webHostBuilder => webHostBuilder.UseStartup<Startup>());
        }
    }

## Local testing

#### Add reference to `vostok.hosting` module

    cm ref add vostok.hosting MyProject/MyProject.csproj

#### Add reference to `vostok.hosting.kontur` module (optional)
    cm ref add vostok.hosting.kontur MyProject/MyProject.csproj

#### Use VostokHost and setup environemnt

    public static async Task Main()
    {
        var application = new MyAspNetCoreApplication();

        VostokHostingEnvironmentSetup environmentSetup = setup =>
        {
            setup
                .SetupApplicationIdentity(
                    applicationIdentityBuilder => applicationIdentityBuilder
                        .SetProject(project)
                        .SetApplication(application)
                )
                .SetupLog(
                    (logBuilder, setupContext) => logBuilder
                        .SetupConsoleLog())
                .SetPort(port);
        };

        var host = new VostokHost(new VostokHostSettings(application, environmentSetup).SetupForKontur());

        await host.WithConsoleCancellation().RunAsync();
    }

#### Run

    dotnet run -p MyProject/MyProject.csproj


    
