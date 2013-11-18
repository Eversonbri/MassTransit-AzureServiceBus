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
namespace MassTransit.Transports.AzureServiceBus
{
    using System;
    using BusConfigurators;
    using Configuration;
    using Configuration.Configurators;
    using EndpointConfigurators;
    using Exceptions;
    using Pipeline.Configuration;
    using Util;


    public static class AzureServiceBusConfigurationExtensions
    {
        /// <summary>
        /// Specifies that MT should be using Azure ServiceBus Queues.
        /// </summary>
        public static T UseAzureServiceBus<T>(this T configurator)
            where T : EndpointFactoryConfigurator
        {
            return UseAzureServiceBus(configurator, x => { });
        }

        /// <summary>
        /// Specifies that MT should be using Azure ServiceBus Queues
        /// and allows you to configure custom settings.
        /// </summary>
        public static T UseAzureServiceBus<T>(this T configurator,
            [NotNull] Action<AzureServiceBusTransportFactoryConfigurator> configure)
            where T : EndpointFactoryConfigurator
        {
            if (configure == null)
                throw new ArgumentNullException("configure");

            var transportConfigurator = new AzureServiceBusFactoryConfiguratorImpl();

            configure(transportConfigurator);

            configurator.AddTransportFactory(transportConfigurator.Configure);

            configurator.UseJsonSerializer();

            return configurator;
        }

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
                    var inboundTransport = sb.Endpoint.InboundTransport as InboundAzureServiceBusTransport;
                    if (inboundTransport == null)
                        throw new ConfigurationException("The inbound transport is not an Azure Service Bus transport");

                    return new TopicSubscriptionObserver(inboundTransport, inboundTransport.MessageNameFormatter);
                });

            var busConf = new PostCreateBusBuilderConfigurator(bus =>
                {
                    var interceptorConf = new OutboundMessageInterceptorConfigurator(bus.OutboundPipeline);

                    interceptorConf.Create(new PublishEndpointInterceptor(bus));
                });

            configurator.AddBusConfigurator(busConf);
        }

        /// <summary>
        /// Configure the settings for a service bus host, such as the shared secret
        /// </summary>
        /// <param name="configurator"></param>
        /// <param name="hostAddress"></param>
        /// <param name="configure"></param>
        public static void ConfigureNamespace(this AzureServiceBusTransportFactoryConfigurator configurator, string ns,
            Action<ConnectionSettingsConfigurator> configure)
        {
            var connectionSettingsConfigurator =new NamespaceConnectionSettingsConfigurator(ns);
            configure(connectionSettingsConfigurator);

            configurator.AddConfigurator(connectionSettingsConfigurator);
        }
    }
}