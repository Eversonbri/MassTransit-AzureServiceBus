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
    using Configuration;
    using Logging;
    using Microsoft.ServiceBus.Messaging;


    /// <summary>
    ///     A consumer for receiving messages pushed from Azure Service Bus to MassTransit
    /// </summary>
    public class Consumer :
        ConnectionBinding<AzureServiceBusConnection>
    {
        static readonly ILog _log = Logger.Get<Consumer>();
        readonly IAzureServiceBusEndpointAddress _address;
        MessageReceiver _receiver;

        /// <summary>
        ///     c'tor for consumer
        /// </summary>
        /// <param name="address">The address configured for the consumer</param>
        /// <param name="inboundSettings"></param>
        public Consumer(IAzureServiceBusEndpointAddress address, IInboundSettings inboundSettings)
        {
            _address = address;
        }

        void ConnectionBinding<AzureServiceBusConnection>.Bind(AzureServiceBusConnection connection)
        {
            MessageReceiver receiver = null;
            try
            {
                BindQueue(connection);

                receiver = connection.MessagingFactory.CreateMessageReceiver(_address.QueueName, ReceiveMode.PeekLock);
                receiver.PrefetchCount = _address.PrefetchCount;

                _receiver = receiver;
            }
            catch (Exception ex)
            {
                if (receiver != null)
                {
                    try
                    {
                        receiver.Close();
                    }
                    catch
                    {
                    }
                }

                throw new InvalidConnectionException(_address.Uri, "Invalid connection to host", ex);
            }
        }

        void ConnectionBinding<AzureServiceBusConnection>.Unbind(AzureServiceBusConnection connection)
        {
            if (_receiver != null)
            {
                try
                {
                    _receiver.Close();
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to close receiver", ex);
                }
                finally
                {
                    _receiver = null;
                }
            }
        }

        void BindQueue(AzureServiceBusConnection connection)
        {
            try
            {
                connection.CreateQueue(_address.QueueName);
            }
            catch (MessagingEntityAlreadyExistsException)
            {
            }
        }

        public BrokeredMessage Get(TimeSpan timeout)
        {
            if (_receiver == null)
                throw new InvalidConnectionException(_address.Uri, "No connection to Azure Service Bus Host");

            BrokeredMessage brokeredMessage = _receiver.Receive(timeout);

            return brokeredMessage;
        }
    }
}