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
    using System.Collections.Generic;
    using Microsoft.ServiceBus;
    using Receiver;


    /// <summary>
    /// Extensible class for managing an azure endpoint/topics. A single virtual method
    /// that can be overridden, the <see cref="CreateTopicsForPublisher"/> (and <see cref="Dispose(bool)"/>).
    /// </summary>
    public class AzureManagementEndpointManagement : IDisposable
    {
        readonly IAzureServiceBusEndpointAddress _address;

        public AzureManagementEndpointManagement(IAzureServiceBusEndpointAddress address)
        {
            _address = address;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual IEnumerable<Type> CreateTopicsForPublisher(Type messageType, IMessageNameFormatter formatter)
        {
            NamespaceManager nm = _address.NamespaceManager;

            foreach (Type type in messageType.GetMessageTypes())
            {
                string topic = formatter.GetMessageName(type).ToString();

                /*
				 * Type here is both the actual message type and its 
				 * interfaces. In RMQ we could bind the interface type
				 * exchanges to the message type exchange, but because this
				 * is azure service bus, we only have plain topics.
				 * 
				 * This means that for a given subscribed message, we have to
				 * subscribe also to all of its interfaces and their corresponding
				 * topics. In this method, it means that we'll just create
				 * ALL of the types' corresponding topics and publish to ALL of them.
				 * 
				 * On the receiving side we'll have to de-duplicate 
				 * the received messages, potentially (unless we're subscribing only
				 * to one of the interfaces that itself doesn't implement any interfaces).
				 */

                nm.CreateAsync(new TopicDescriptionImpl(topic)).Wait();

                yield return type;
            }
        }

        protected virtual void Dispose(bool managed)
        {
        }
    }
}