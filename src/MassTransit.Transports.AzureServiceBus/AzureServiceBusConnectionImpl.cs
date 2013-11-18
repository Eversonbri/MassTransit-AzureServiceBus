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
    using Exceptions;
    using Logging;
    using Magnum.Extensions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.ServiceBus.Messaging.Amqp;


    /// <summary>
    ///     Connection to Azure Service Bus message broker.
    /// </summary>
    public class AzureServiceBusConnectionImpl :
        AzureServiceBusConnection
    {
        static readonly ILog _log = Logger.Get(typeof(AzureServiceBusConnectionImpl));
        readonly IAzureServiceBusEndpointAddress _address;
        readonly Uri _serviceUri;
        readonly TokenProvider _tokenProvider;

        bool _disposed;

        MessagingFactory _factory;
        NamespaceManager _manager;

        /// <summary>
        ///     A connection is stored per connection string
        /// </summary>
        /// <param topicName="address">The address for the connection</param>
        /// <param topicName="tokenProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureServiceBusConnectionImpl(IAzureServiceBusEndpointAddress address, TokenProvider tokenProvider)
        {
            if (address == null)
                throw new ArgumentNullException("address");

            _address = address;
            _tokenProvider = tokenProvider;

            _serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", _address.Namespace, string.Empty);

            /*When using the default lock expiration of 60 seconds, a good value for SubscriptionClient.PrefetchCount
			 * is 20 times the maximum processing rates of all receivers of the factory. For example,
			 * a factory creates 3 receivers. Each receiver can process up to 10 messages per second.
			 * The prefetch count should not exceed 20*3*10 = 600.By default, QueueClient.PrefetchCount
			 * is set to 0, which means that no additional messages are fetched from the service. */

            _log.DebugFormat("Connection '{0}' created at {1}", _address, _serviceUri);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                Disconnect();
            }
            finally
            {
                _disposed = true;
            }
        }

        public void Connect()
        {
            Disconnect();

            _log.DebugFormat("Connecting to '{0}' at {1}", _address, _serviceUri);

            _manager = new NamespaceManager(_serviceUri, _tokenProvider);

            var mfs = new MessagingFactorySettings
            {
                TokenProvider = _tokenProvider,
                OperationTimeout = 3.Seconds(),
                TransportType = TransportType.Amqp,
                AmqpTransportSettings = new AmqpTransportSettings
                {
                    BatchFlushInterval = 50.Milliseconds()
                },
            };

            _factory = MessagingFactory.Create(_serviceUri, mfs);
        }

        public void Disconnect()
        {
            if (_factory != null)
            {
                _log.DebugFormat("disconnecting '{0}'", _address);

                _factory.Close();
                _factory = null;
            }

            _manager = null;
        }

        public MessagingFactory MessagingFactory
        {
            get
            {
                if (_factory == null)
                    throw new TransportException(_address.Uri, "The messaging factory is not available");

                return _factory;
            }
        }

        public NamespaceManager NamespaceManager
        {
            get
            {
                if (_manager == null)
                    throw new TransportException(_address.Uri, "The namespace manager is not available");

                return _manager;
            }
        }

        public void CreateQueue(string queueName)
        {
            if (_manager == null)
                throw new TransportException(_address.Uri, "The namespace manager is not available");

            var description = new QueueDescription(queueName)
            {
                DefaultMessageTimeToLive = _address.DefaultMessageTimeToLive,
                EnableBatchedOperations = _address.EnableBatchOperations,
                LockDuration = _address.LockDuration,
                MaxDeliveryCount = _address.MaxDeliveryCount,
            };

            try
            {
                _manager.CreateQueue(description);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
            }
        }

        public void CreateTopic(string topicName)
        {
            if (_manager == null)
                throw new TransportException(_address.Uri, "The namespace manager is not available");

            var description = new TopicDescription(topicName)
            {
                DefaultMessageTimeToLive = _address.DefaultMessageTimeToLive,
                EnableBatchedOperations = _address.EnableBatchOperations,
            };

            try
            {
                _manager.CreateTopic(description);
            }
            catch (MessagingEntityAlreadyExistsException ex)
            {
            }
        }
    }
}