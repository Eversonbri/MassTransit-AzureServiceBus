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
namespace MassTransit.Transports.AzureServiceBus.Configuration.Configurators
{
    using System;


    /// <summary>
    /// 	Interface for users to configure the transport with
    /// </summary>
    public interface AzureServiceBusTransportFactoryConfigurator
    {
        /// <summary>
        /// 	Sets the lock duration for the messages consumed from queues and topics, which affects the rate of duplicates.
        ///  The lower the duration, the more duplicates you'll get for processes that never finish before the lease
        ///  expires (these messages will pop up again).
        /// </summary>
        void SetLockDuration(TimeSpan lockDuration);

        /// <summary>
        /// 	Sets the default message time to live, i.e. how long it will remain in a queue or topic before being removed by the broker.
        /// </summary>
        void SetDefaultMessageTimeToLive(TimeSpan ttl);

        /// <summary>
        /// 	Sets whether the message should be moved to the built-in AZURE (not MassTransit) error poison message queue if 
        /// the message is expired, i.e. if the message is NOT consumed by a consumer.
        /// </summary>
        void SetDeadLetteringOnExpiration(bool enabled);

        /// <summary>
        /// 	Sets whether batched operations are enabled on topics and queues.
        /// </summary>
        void SetBatchedOperations(bool enabled);

        /// <summary>
        /// 	Set the receiver name (this corresponds to the name of the subscription created on topics in Azure ServiceBus. 
        /// Setting this equal to what another bus has, allows your bus to do competing consumes on all message types that it consumes.
        /// </summary>
        /// <exception cref="ArgumentException">name.trim() is an empty string</exception>
        /// <param name="name"> Name to use for subscriptions. </param>
        void SetReceiverName(string name);

        /// <summary>
        /// 	Sets the timeout for receiving a message using a single operation.
        /// </summary>
        void SetReceiveTimeout(TimeSpan timeout);

        /// <summary>
        ///		Sets the number of outstanding send operations to tolerate in the outbound
        /// transports (outbound and error transports that is).
        /// </summary>
        /// <param name="number"></param>
        void SetMaxOutstandingSendOperations(int number);

        /// <summary>
        /// Add a builder configurator
        /// </summary>
        /// <param name="configurator"></param>
        void AddConfigurator(AzureServiceBusTransportFactoryBuilderConfigurator configurator);
    }
}