// Copyright 2012 Henrik Feldt
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

#pragma warning disable 1591

namespace MassTransit.Transports.AzureServiceBus.Configuration
{
    using System;
    using EndpointConfigurators;
    using Util;


    public static class EndpointFactoryConfiguratorExtensions
    {
        /// <summary>
        /// Specifies that MT should be using AppFabric ServiceBus Queues.
        /// </summary>
        public static T UseAzureServiceBus<T>(this T configurator)
            where T : EndpointFactoryConfigurator
        {
            return UseAzureServiceBus(configurator, x => { });
        }

        /// <summary>
        /// Specifies that MT should be using AppFabric ServiceBus Queues
        /// and allows you to configure custom settings.
        /// </summary>
        public static T UseAzureServiceBus<T>(this T configurator,
            [NotNull] Action<AzureServiceBusFactoryConfigurator> configure)
            where T : EndpointFactoryConfigurator
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            var tfacCfg = new AzureAzureServiceBusFactoryConfiguratorImpl();

            configure(tfacCfg);

            configurator.AddTransportFactory(tfacCfg.Build);

            configurator.UseJsonSerializer();

            return configurator;
        }
    }
}