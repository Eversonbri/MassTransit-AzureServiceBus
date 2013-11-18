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
namespace MassTransit.Transports.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Exceptions;
    using Logging;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;


    public class Producer :
        ConnectionBinding<AzureServiceBusConnection>
    {
        static readonly ILog _log = Logger.Get(typeof(Producer));
        readonly IAzureServiceBusEndpointAddress _address;
        readonly object _lock = new object();
        QueueClient _queueClient;
        TopicClient _topicClient;

        public Producer(IAzureServiceBusEndpointAddress address)
        {
            _address = address;
        }

        void ConnectionBinding<AzureServiceBusConnection>.Bind(AzureServiceBusConnection connection)
        {
            lock (_lock)
            {
                if (_address.IsQueue)
                {
                    CreateQueue(connection.NamespaceManager, _address.QueueName);
                    QueueClient queueClient = connection.MessagingFactory.CreateQueueClient(_address.QueueName,
                        ReceiveMode.PeekLock);
                    _queueClient = queueClient;
                }
                else if (_address.IsTopic)
                {
                    CreateTopic(connection.NamespaceManager, _address.TopicName);
                    TopicClient queueClient = connection.MessagingFactory.CreateTopicClient(_address.TopicName);

                    _topicClient = queueClient;
                }
                else
                    throw new ConfigurationException("The address is neither a queue nor a topic");
            }
        }

        void ConnectionBinding<AzureServiceBusConnection>.Unbind(AzureServiceBusConnection connection)
        {
            lock (_lock)
            {
                try
                {
                    if (_topicClient != null && !_topicClient.IsClosed)
                        _topicClient.Close();

                    if (_queueClient != null && !_queueClient.IsClosed)
                        _queueClient.Close();
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to close client", ex);
                }
                finally
                {
                    _topicClient = null;
                    _queueClient = null;
                }
            }
        }

        public void Send(BrokeredMessage message)
        {
            lock (_lock)
            {
                if (_queueClient != null)
                    _queueClient.Send(message);
                else if (_topicClient != null)
                    _topicClient.Send(message);
                else
                    throw new InvalidConnectionException(_address.Uri, "No connection to Azure Service Bus Host");
            }
        }

        public Task SendAsync(BrokeredMessage message)
        {
            lock (_lock)
            {
                if (_queueClient != null)
                    return _queueClient.SendAsync(message);
                if (_topicClient != null)
                    return _topicClient.SendAsync(message);

                throw new InvalidConnectionException(_address.Uri, "No connection to Azure Service Bus Host");
            }
        }

        void CreateQueue(NamespaceManager manager, string queueName)
        {
            var description = new QueueDescription(queueName)
                {
                    DefaultMessageTimeToLive = _address.DefaultMessageTimeToLive,
                    EnableBatchedOperations = _address.EnableBatchOperations,
                    LockDuration = _address.LockDuration,
                    MaxDeliveryCount = _address.MaxDeliveryCount,
                };

            manager.CreateQueue(description);
        }

        void CreateTopic(NamespaceManager manager, string topicName)
        {
            var description = new TopicDescription(topicName)
                {
                    DefaultMessageTimeToLive = _address.DefaultMessageTimeToLive,
                    EnableBatchedOperations = _address.EnableBatchOperations,
                };

            manager.CreateTopic(description);
        }
    }
}