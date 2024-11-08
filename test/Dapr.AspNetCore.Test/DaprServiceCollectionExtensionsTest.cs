﻿// ------------------------------------------------------------------------
// Copyright 2021 The Dapr Authors
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ------------------------------------------------------------------------

#nullable enable

using System;
using System.Text.Json;
using Dapr.Client;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Dapr.AspNetCore.Test
{
    public class DaprServiceCollectionExtensionsTest
    {
        [Fact]
        public void AddDaprClient_RegistersDaprClientOnlyOnce()
        {
            var services = new ServiceCollection();

            var clientBuilder = new Action<DaprClientBuilder>(
                builder => builder.UseJsonSerializationOptions(
                    new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = false
                    }
                )
            );

            // register with JsonSerializerOptions.PropertyNameCaseInsensitive = true (default)
            services.AddDaprClient();

            // register with PropertyNameCaseInsensitive = false
            services.AddDaprClient(clientBuilder);

            var serviceProvider = services.BuildServiceProvider();

            DaprClientGrpc? daprClient = serviceProvider.GetService<DaprClient>() as DaprClientGrpc;

            Assert.NotNull(daprClient);
            Assert.True(daprClient?.JsonSerializerOptions.PropertyNameCaseInsensitive);
        }

        [Fact]
        public void AddDaprClient_RegistersUsingDependencyFromIServiceProvider()
        {

            var services = new ServiceCollection();
            services.AddSingleton<TestConfigurationProvider>();
            services.AddDaprClient((provider, builder) =>
            {
                var configProvider = provider.GetRequiredService<TestConfigurationProvider>();
                var caseSensitivity = configProvider.GetCaseSensitivity();

                builder.UseJsonSerializationOptions(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = caseSensitivity
                });
            });

            var serviceProvider = services.BuildServiceProvider();

            DaprClientGrpc? client = serviceProvider.GetRequiredService<DaprClient>() as DaprClientGrpc;

            //Registers with case-insensitive as true by default, but we set as false above
            Assert.NotNull(client);
            Assert.False(client?.JsonSerializerOptions.PropertyNameCaseInsensitive);
        }

        
#if NET8_0_OR_GREATER
        [Fact]
        public void AddDaprClient_WithKeyedServices()
        {
            var services = new ServiceCollection();

            services.AddKeyedSingleton("key1", new Object());

            services.AddDaprClient();

            var serviceProvider = services.BuildServiceProvider();

            var daprClient = serviceProvider.GetService<DaprClient>();

            Assert.NotNull(daprClient);
        }
#endif

        private class TestConfigurationProvider
        {
            public bool GetCaseSensitivity() => false;
        }
    }
}
