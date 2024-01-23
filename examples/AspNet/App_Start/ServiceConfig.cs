// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Examples.AspNet;

public static class ServiceConfig
{
    public static void ConfigureServices(IServiceCollection services)
    {
        // Register MvC controllers
        services.AddTransientRegistrations(typeof(ServiceConfig).Assembly.GetExportedTypes()
           .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition)
           .Where(t => typeof(IController).IsAssignableFrom(t)
              || t.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)));

        // Conigure OpenTelemetry
        ConfigureOpenTelemetry(services);
    }

    public static void ConfigureApplication(IServiceProvider serviceProvider)
    {
        StartOpenTelemetry(serviceProvider);
    }

    private static void ConfigureOpenTelemetry(IServiceCollection services)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService("AspNet Example App");

        // Add TracerProvider into the service collection and configure OpenTelemetry tracing
        services.AddSingleton(sp =>
        {
            var builder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation();

            switch (ConfigurationManager.AppSettings["UseTracingExporter"].ToUpperInvariant())
            {
                case "ZIPKIN":
                    builder.AddZipkinExporter(zipkinOptions =>
                    {
                        zipkinOptions.Endpoint = new Uri(ConfigurationManager.AppSettings["ZipkinEndpoint"]);
                    });
                    break;
                case "OTLP":
                    builder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(ConfigurationManager.AppSettings["OtlpEndpoint"]);
                    });
                    break;
                default:
                    builder.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug);
                    break;
            }

            return builder.Build();
        });

        // Add MeterProvider into the service collection and configure OpenTelemetry metrics
        services.AddSingleton(sp =>
        {
            // Metrics
            // Note: Tracerprovider is needed for metrics to work
            // https://github.com/open-telemetry/opentelemetry-dotnet/issues/2994

            var meterBuilder = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddAspNetInstrumentation();

            switch (ConfigurationManager.AppSettings["UseMetricsExporter"].ToUpperInvariant())
            {
                case "OTLP":
                    meterBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(ConfigurationManager.AppSettings["OtlpEndpoint"]);
                    });
                    break;
                case "PROMETHEUS":
                    meterBuilder.AddPrometheusHttpListener();
                    break;
                default:
                    meterBuilder.AddConsoleExporter((exporterOptions, metricReaderOptions) =>
                    {
                        exporterOptions.Targets = ConsoleExporterOutputTargets.Debug;
                    });
                    break;
            }

            return meterBuilder.Build();
        });

        // Add ILoggerFactory into the service collection and configure OpenTelemetry logging
        services.AddLogging(builder => builder
            .AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);

                switch (ConfigurationManager.AppSettings["UseLoggingExporter"].ToUpperInvariant())
                {
                    case "OTLP":
                        options.AddOtlpExporter(otlpOptions =>
                        {
                            otlpOptions.Endpoint = new Uri(ConfigurationManager.AppSettings["OtlpEndpoint"]);
                        });
                        break;
                    default:
                        options.AddConsoleExporter(options => options.Targets = ConsoleExporterOutputTargets.Debug);
                        break;
                }
            }));
    }

    private static void StartOpenTelemetry(IServiceProvider serviceProvider)
    {
        // Request OpenTelemetry providers from the service provider so that they are started with the application
        serviceProvider.GetRequiredService<TracerProvider>();
        serviceProvider.GetRequiredService<MeterProvider>();
    }

    private static IServiceCollection AddTransientRegistrations(
        this IServiceCollection services,
        IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            services.AddTransient(type);
        }

        return services;
    }
}
