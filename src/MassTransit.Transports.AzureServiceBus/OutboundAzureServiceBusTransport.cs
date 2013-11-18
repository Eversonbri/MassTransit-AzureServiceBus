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
    using System.IO;
    using Configuration;
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using Util;


    /// <summary>
    /// 	Outbound transport targeting the azure service bus.
    /// </summary>
    public class OutboundAzureServiceBusTransport :
        IOutboundTransport
    {
        static readonly ILog _logger = Logger.Get<OutboundAzureServiceBusTransport>();

        readonly IAzureServiceBusEndpointAddress _address;
        readonly ConnectionHandler<AzureServiceBusConnection> _connectionHandler;
        bool _disposed;
        Producer _producer;

        /// <summary>
        /// 	c'tor
        /// </summary>
        public OutboundAzureServiceBusTransport( IAzureServiceBusEndpointAddress address,  ConnectionHandler<AzureServiceBusConnection> connectionHandler, IOutboundSettings outboundSettings)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (connectionHandler == null)
                throw new ArgumentNullException("connectionHandler");

            _connectionHandler = connectionHandler;
            _address = address;

            _logger.DebugFormat("created outbound transport for address '{0}'", address);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            RemoveProducer();

            _disposed = true;
        }

        /// <summary>
        /// 	Gets the endpoint address this transport sends to.
        /// </summary>
        public IEndpointAddress Address
        {
            get { return _address; }
        }

        // service bus best practices for performance:
        // http://msdn.microsoft.com/en-us/library/windowsazure/hh528527.aspx
        public void Send(ISendContext context)
        {
            AddProducerBinding();

            _connectionHandler.Use(connection =>
                {
                    try
                    {
                        // don't have too many outstanding at same time
//                        SpinWait.SpinUntil(() => _messagesInFlight < _settings.MaxOutstanding);

                        using (var body = new MemoryStream())
                        {
                            context.SerializeTo(body);

                            body.Seek(0, SeekOrigin.Begin);

                            // TODO put transient handling logic in here for retry

                            var brokeredMessage = new BrokeredMessage(body, false);

                            brokeredMessage.MessageId = context.MessageId
                                                        ?? brokeredMessage.MessageId ?? NewId.Next().ToString();
                            if (context.ExpirationTime.HasValue)
                            {
                                DateTime value = context.ExpirationTime.Value;
                                brokeredMessage.TimeToLive =
                                    (value.Kind == DateTimeKind.Utc
                                         ? value - DateTime.UtcNow
                                         : value - DateTime.Now);
                            }

                            if (!string.IsNullOrWhiteSpace(context.CorrelationId))
                                brokeredMessage.CorrelationId = context.CorrelationId;

                            foreach (var header in context.Headers)
                                brokeredMessage.Properties.Add(header.Key, header.Value);

                            brokeredMessage.ContentType = context.ContentType;

                            _producer.Send(brokeredMessage);
                        }
                    }
                    catch (TimeoutException ex)
                    {
                        throw new InvalidConnectionException(_address.Uri, "Send operation timed out", ex);
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new InvalidConnectionException(_address.Uri, "Operation was cancelled", ex);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidConnectionException(_address.Uri, "Send threw an exception", ex);
                    }
                });
        }

        void RemoveProducer()
        {
            if (_producer != null)
                _connectionHandler.RemoveBinding(_producer);
        }

        void AddProducerBinding()
        {
            if (_producer != null)
                return;

            _producer = new Producer(_address);

            _connectionHandler.AddBinding(_producer);
        }
    }
}