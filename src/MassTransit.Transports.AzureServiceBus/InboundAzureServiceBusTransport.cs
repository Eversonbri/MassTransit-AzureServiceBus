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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Configuration;
    using Context;
    using Logging;
    using Microsoft.ServiceBus.Messaging;


    /// <summary>
    ///     Inbound transport implementation for Azure Service Bus.
    /// </summary>
    public class InboundAzureServiceBusTransport :
        IInboundTransport
    {
        static readonly ILog _logger = Logger.Get<InboundAzureServiceBusTransport>();
        readonly IAzureServiceBusEndpointAddress _address;
        readonly ConnectionHandler<AzureServiceBusConnection> _connectionHandler;
        readonly IMessageNameFormatter _formatter;
        readonly IInboundSettings _inboundSettings;
        Consumer _consumer;
        bool _disposed;
        Publisher _publisher;

        public InboundAzureServiceBusTransport(IAzureServiceBusEndpointAddress address,
            ConnectionHandler<AzureServiceBusConnection> connectionHandler,
            IMessageNameFormatter formatter = null,
            IInboundSettings inboundSettings = null)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (connectionHandler == null)
                throw new ArgumentNullException("connectionHandler");

            _address = address;
            _connectionHandler = connectionHandler;
            _formatter = formatter ?? new AzureServiceBusMessageNameFormatter();
            _inboundSettings = inboundSettings;

            _logger.DebugFormat("created new inbound transport for '{0}'", address);
        }

        /// <summary>
        ///     The formatter for message types
        /// </summary>
        public IMessageNameFormatter MessageNameFormatter
        {
            get { return _formatter; }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _logger.DebugFormat("disposing transport for '{0}'", Address);

            try
            {
                RemoveConsumerBinding();
            }
            finally
            {
                _disposed = true;
            }
        }

        public IEndpointAddress Address
        {
            get { return _address; }
        }

        public void Receive(Func<IReceiveContext, Action<IReceiveContext>> callback, TimeSpan timeout)
        {
            AddConsumerBinding();

            _connectionHandler.Use(connection =>
                {
                    BrokeredMessage message = _consumer.Get(timeout);
                    if (message == null)
                        return;

                    using (var stream = message.GetBody<Stream>())
                    {
                        ReceiveContext context = ReceiveContext.FromBodyStream(stream);
                        context.SetMessageId(message.MessageId);
                        context.SetInputAddress(Address);
                        context.SetCorrelationId(message.CorrelationId);
                        context.SetContentType(message.ContentType);

                        Action<IReceiveContext> receive = callback(context);
                        if (receive == null)
                        {
                            Address.LogSkipped(message.MessageId);
                            return;
                        }

                        try
                        {
                            receive(context);
                        }
                        catch (Exception ex)
                        {
                            if (_logger.IsErrorEnabled)
                                _logger.Error("Consumer threw an exception", ex);

                            message.Abandon();
                        }

                        try
                        {
                            message.Complete();
                        }
                        catch (MessageLockLostException ex)
                        {
                            if (_logger.IsErrorEnabled)
                                _logger.Error("Message Lock Lost on message Complete()", ex);
                        }
                        catch (MessagingException ex)
                        {
                            if (_logger.IsErrorEnabled)
                                _logger.Error("Generic MessagingException thrown", ex);
                        }
                    }
                });
        }

        public IEnumerable<Type> SubscribeTopicsForPublisher(Type messageType,
            IMessageNameFormatter messageNameFormatter)
        {
            AddPublisherBinding();

            IList<Type> messageTypes = new List<Type>();
            _connectionHandler.Use(connection =>
                {
                    MessageName messageName = messageNameFormatter.GetMessageName(messageType);

                    _publisher.CreateTopic(messageName.ToString());
                    messageTypes.Add(messageType);

                    foreach (Type type in messageType.GetMessageTypes().Skip(1))
                    {
                        MessageName interfaceName = messageNameFormatter.GetMessageName(type);
                        
                        // Create topics for inherited types before trying to setup the subscription
                        _publisher.CreateTopic(interfaceName.Name.ToString());

                        _publisher.AddTopicSubscription(interfaceName.ToString(), messageName.ToString());
                        messageTypes.Add(type);
                    }
                });

            return messageTypes;
        }

        void AddPublisherBinding()
        {
            if (_publisher != null)
                return;

            _publisher = new Publisher(_address);

            _connectionHandler.AddBinding(_publisher);
        }


        void AddConsumerBinding()
        {
            if (_consumer != null)
                return;

            _consumer = new Consumer(_address, _inboundSettings);

            _connectionHandler.AddBinding(_consumer);
        }

        void RemoveConsumerBinding()
        {
            if (_consumer == null)
                return;

            _connectionHandler.RemoveBinding(_consumer);
            _consumer = null;
        }

        public void AddTopicSubscriber(TopicDescription value)
        {
            AddPublisherBinding();
            _publisher.AddTopicSubscription(_address.QueueName, value.Path);
        }

        public void RemoveTopicSubscriber(TopicDescription value)
        {
            AddPublisherBinding();
            _publisher.RemoveTopicSubscription(_address.QueueName, value.Path);
        }
    }
}