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
    using BusConfigurators;
    using Magnum.Extensions;
    using Pipeline.Configuration;
    using Util;


    public static class AzureServiceBusServiceBusExtensions
    {
        /// <summary>
        /// Specifies that MT should be using AppFabric ServiceBus Queues to receive messages and specifies the
        /// uri by means of its components.
        /// </summary>
        public static void ReceiveFromComponents<T>(this T configurator,
            [NotNull] string issuerOrUsername,
            [NotNull] string defaultKeyOrPassword,
            [NotNull] string serviceBusNamespace,
            [NotNull] string application)
            where T : ServiceBusConfigurator
        {
            if (issuerOrUsername == null)
                throw new ArgumentNullException("issuerOrUsername");
            if (defaultKeyOrPassword == null)
                throw new ArgumentNullException("defaultKeyOrPassword");
            if (serviceBusNamespace == null)
                throw new ArgumentNullException("serviceBusNamespace");
            if (application == null)
                throw new ArgumentNullException("application");
            var credentials = new Credentials(issuerOrUsername, defaultKeyOrPassword, serviceBusNamespace, application);
            configurator.ReceiveFrom(credentials.BuildUri());
        }

        /// <summary>
        /// Configure MassTransit to consume from Azure Service Bus.
        /// </summary>
        public static void ReceiveFromComponents<T>(this T configurator,
            [NotNull] PreSharedKeyCredentials creds)
            where T : ServiceBusConfigurator
        {
            if (creds == null)
                throw new ArgumentNullException("creds");
            configurator.ReceiveFrom(creds.BuildUri());
        }

        /// <summary>
        /// Configure the service bus to use the queues and topics routing semantics with
        /// Azure ServiceBus.
        /// </summary>
        public static void UseAzureServiceBusRouting<T>(this T configurator)
            where T : ServiceBusConfigurator
        {
            configurator.SetSubscriptionObserver((sb, router) =>
                {
                    var inboundTransport = sb.Endpoint.InboundTransport.CastAs<AzureServiceBusInboundTransport>();
                    return new TopicSubscriptionObserver(inboundTransport.MessageNameFormatter, inboundTransport);
                });

            var busConf = new PostCreateBusBuilderConfigurator(bus =>
                {
                    var interceptorConf = new OutboundMessageInterceptorConfigurator(bus.OutboundPipeline);

                    interceptorConf.Create(new PublishEndpointInterceptor(bus));
                });

            configurator.AddBusConfigurator(busConf);

            // configurator.UseAzureServiceBus();
        }
    }
}