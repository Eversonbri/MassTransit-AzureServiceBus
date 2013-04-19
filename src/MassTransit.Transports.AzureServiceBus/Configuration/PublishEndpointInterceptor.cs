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
namespace MassTransit.Transports.AzureServiceBus.Configuration
{
    using System;
    using System.Collections.Generic;
    using Exceptions;
    using Magnum.Extensions;
    using Magnum.Reflection;
    using Pipeline.Configuration;
    using Pipeline.Sinks;
    using Util;


    /// <summary>
    /// Interceptor for bus.Publish calls; look in the endpoints of the bus for 
    /// endpoints of the published message type. Highly cohesive with <see cref="PublishEndpointSinkLocator"/>.
    /// </summary>
    public class PublishEndpointInterceptor : IOutboundMessageInterceptor
    {
        readonly Dictionary<Type, UnsubscribeAction> _added;
        readonly IAzureServiceBusEndpointAddress _address;
        readonly ServiceBus _bus;
        readonly IMessageNameFormatter _formatter;

        /// <summary>
        /// c'tor
        /// </summary>
        /// <param name="bus"></param>
        public PublishEndpointInterceptor([NotNull] ServiceBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException("bus");

            _bus = bus;

            var inbound = bus.Endpoint.InboundTransport as AzureServiceBusInboundTransport;

            if (inbound == null)
            {
                throw new ConfigurationException(
                    "The bus must be configured to receive from an Azure ServiceBus Endpoint for this interceptor to work.");
            }

            _formatter = inbound.MessageNameFormatter;
            _address = inbound.Address.CastAs<IAzureServiceBusEndpointAddress>();
            _added = new Dictionary<Type, UnsubscribeAction>();
        }

        void IOutboundMessageInterceptor.PreDispatch(ISendContext context)
        {
            lock (_added)
            {
                Type messageType = context.DeclaringMessageType;

                if (_added.ContainsKey(messageType))
                    return;

                AddEndpointForType(messageType);
            }
        }

        void IOutboundMessageInterceptor.PostDispatch(ISendContext context)
        {
        }

        /// <summary>
        /// Adds an endpoint for the message type. This will look up all super-classes
        /// of the message's type (running for those as well) and then create
        /// message sinks corresponding to the type of message that is being published.
        /// </summary>
        /// <param name="messageType">The type of message to add an endpoint for.</param>
        void AddEndpointForType(Type messageType)
        {
            using (var management = new AzureManagementEndpointManagement(_address))
            {
                IEnumerable<Type> types = management.CreateTopicsForPublisher(messageType, _formatter);

                foreach (Type type in types)
                {
                    if (_added.ContainsKey(type))
                        continue;

                    MessageName messageName = _formatter.GetMessageName(type);

                    IAzureServiceBusEndpointAddress messageEndpointAddress = _address.ForTopic(messageName.ToString());

                    FindOrAddEndpoint(type, messageEndpointAddress);
                }
            }
        }

        /// <summary>
        /// Finds all endpoints in the outbound pipeline and starts routing messages
        /// to that endpoint.
        /// </summary>
        /// <param name="messageType">type of message</param>
        /// <param name="address">The message endpoint address.</param>
        void FindOrAddEndpoint(Type messageType, IAzureServiceBusEndpointAddress address)
        {
            var locator = new PublishEndpointSinkLocator(messageType, address);
            _bus.OutboundPipeline.Inspect(locator);

            if (locator.Found)
            {
                _added.Add(messageType, () => true);
                // subscribed sink exists already, returning
                return;
            }

            // otherwise, get the endpoint and add a sink for it
            IEndpoint endpoint = _bus.GetEndpoint(address.Uri);

            this.FastInvoke(new[] {messageType}, "CreateEndpointSink", endpoint);
        }

        /// <summary>
        /// Actually create a new sink; the sink didn't exist in the outbound
        /// pipeline, so we need to create a new one.
        /// </summary>
        /// <typeparam name="TMessage">Message type to create the sink for.</typeparam>
        /// <param name="endpoint">The endpoint to attach the message sink to.</param>
        [UsedImplicitly]
        void CreateEndpointSink<TMessage>(IEndpoint endpoint)
            where TMessage : class
        {
            var endpointSink = new EndpointMessageSink<TMessage>(endpoint);

            var filterSink = new OutboundMessageFilter<TMessage>(endpointSink,
                context => context.DeclaringMessageType == typeof(TMessage));

            UnsubscribeAction unsub = _bus.OutboundPipeline.ConnectToRouter(filterSink);

            _added.Add(typeof(TMessage), unsub);
        }
    }
}