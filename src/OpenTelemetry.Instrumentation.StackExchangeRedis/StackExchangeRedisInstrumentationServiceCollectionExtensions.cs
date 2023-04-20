// <copyright file="StackExchangeRedisInstrumentationServiceCollectionExtensions.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Internal;

namespace OpenTelemetry.Instrumentation.StackExchangeRedis;

public static class StackExchangeRedisInstrumentationServiceCollectionExtensions
{
    public static IServiceCollection AddRedisInstrumentationCreatedAction(
        this IServiceCollection services,
        Action<StackExchangeRedisInstrumentation> instrumenationCreatedAction)
    {
        Guard.ThrowIfNull(instrumenationCreatedAction);

        return services.AddRedisInstrumentationCreatedAction((sp, n, i) => instrumenationCreatedAction(i));
    }

    public static IServiceCollection AddRedisInstrumentationCreatedAction(
        this IServiceCollection services,
        Action<string, StackExchangeRedisInstrumentation> instrumenationCreatedAction)
    {
        Guard.ThrowIfNull(instrumenationCreatedAction);

        return services.AddRedisInstrumentationCreatedAction((sp, n, i) => instrumenationCreatedAction(n, i));
    }

    public static IServiceCollection AddRedisInstrumentationCreatedAction(
        this IServiceCollection services,
        Action<IServiceProvider, string, StackExchangeRedisInstrumentation> instrumenationCreatedAction)
    {
        Guard.ThrowIfNull(services);
        Guard.ThrowIfNull(instrumenationCreatedAction);

        services.AddSingleton<StackExchangeRedisInstrumentationAction>(sp =>
            (n, i) => instrumenationCreatedAction(sp, n, i));

        return services;
    }
}
