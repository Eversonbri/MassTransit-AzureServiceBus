// Copyright 2012 Henrik Feldt, Chris Patterson, et. al.
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
namespace MassTransit.Testing
{
    using System;
    using TestInstanceConfigurators;
    using Transports.AzureServiceBus.Configuration;


    public static class BusTestScenarioExtensions
    {
        /// <summary>
        ///     Create a new testing scenario with Azure Service Bus.
        ///     Note: currently recommended to use the loopback scenario builder.
        /// </summary>
        public static void UseAzureServiceBusBusScenario(this TestInstanceConfigurator<BusTestScenario> configurator,
            SharedAccessSignatureSettings settings)
        {
            if (configurator == null)
                throw new ArgumentNullException("configurator");
            if (settings == null)
                throw new ArgumentNullException("settings");

            configurator.UseScenarioBuilder(() => new AzureServiceBusScenarioBuilder(settings));
        }
    }
}