// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Web;
using System.Web.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Examples.AspNet;

public partial class WebApiApplication : HttpApplication
{
    private static ServiceProvider? serviceProvider;

    public static IServiceProvider ServiceProvider => serviceProvider
        ?? throw new InvalidOperationException("ServiceProvider cannot be accessed before the application has started.");

    public static ILogger<T> GetLogger<T>()
        => ServiceProvider.GetRequiredService<ILogger<T>>();

    protected void Application_Start()
    {
        var services = new ServiceCollection();

        // Configure OpenTelemetry services
        ServiceConfig.ConfigureServices(services);

        serviceProvider = services.BuildServiceProvider();

        // Start OpenTelemetry services
        ServiceConfig.ConfigureApplication(serviceProvider);

        // Configure WebAPI
        GlobalConfiguration.Configure(WebApiConfig.Register);

        // Configure MVC
        MvcConfig.Configure();

        Logs.ApplicationStarted(GetLogger<WebApiApplication>());
    }

    protected void Application_End()
    {
        Logs.ApplicationStopping(GetLogger<WebApiApplication>());

        // Dispose OpenTelemetry services and flush anything in memory
        serviceProvider?.Dispose();
    }

    private static partial class Logs
    {
        [LoggerMessage(LogLevel.Information, "Application started")]
        public static partial void ApplicationStarted(ILogger logger);

        [LoggerMessage(LogLevel.Information, "Application stopping")]
        public static partial void ApplicationStopping(ILogger logger);
    }
}
