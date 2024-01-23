// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Examples.AspNet;

public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // Web API configuration and services

        // Web API routes
        config.MapHttpAttributeRoutes();

        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional });

        config.Formatters.Clear();
        config.Formatters.Add(new JsonMediaTypeFormatter());

        config.DependencyResolver = new WebApiServiceProviderDependencyResolver(
            serviceScope: null, serviceProvider: WebApiApplication.ServiceProvider);
    }

    private sealed class WebApiServiceProviderDependencyResolver : IDependencyResolver
    {
        private readonly IServiceScope? serviceScope;
        private readonly IServiceProvider serviceProvider;

        public WebApiServiceProviderDependencyResolver(IServiceScope? serviceScope, IServiceProvider serviceProvider)
        {
            this.serviceScope = serviceScope;
            this.serviceProvider = serviceProvider;
        }

        public object GetService(Type serviceType)
        {
            return this.serviceProvider.GetService(serviceType);
        }

        public IEnumerable<object?> GetServices(Type serviceType)
        {
            return this.serviceProvider.GetServices(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            var serviceScope = this.serviceProvider.CreateScope();

            return new WebApiServiceProviderDependencyResolver(serviceScope, serviceScope.ServiceProvider);
        }

        public void Dispose()
        {
            this.serviceScope?.Dispose();
        }
    }
}
