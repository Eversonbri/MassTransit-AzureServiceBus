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
    using Microsoft.ServiceBus;


    public interface IAzureServiceBusEndpointAddress :
        IEndpointAddress
    {
        string QueueName { get; }
        int PrefetchCount { get; }
        TimeSpan DefaultMessageTimeToLive { get; }
        bool EnableBatchOperations { get; }
        TimeSpan LockDuration { get; }
        int MaxDeliveryCount { get; }
        bool IsQueue { get; }
        bool IsTopic { get; }
        string TopicName { get; }
        IConnectionSettings ConnectionSettings { get; }
        string Namespace { get; }

//        [<NotNull>]
//  abstract member TokenProvider : TokenProvider
//  [<NotNull>]
//  abstract member MessagingFactoryFactory : Func<MessagingFactory>
//  [<NotNull>]
//  abstract member NamespaceManager : NamespaceManager
//  [<NotNull>]
//  abstract member CreateQueue : unit -> Task
//  [<CanBeNull>]
//  abstract member QueueDescription : QueueDescription
//  [<CanBeNull>]
//  abstract member TopicDescription : TopicDescription
//  [<NotNull>]
//  abstract member ForTopic : string -> IAzureServiceBusEndpointAddress
        IAzureServiceBusEndpointAddress ForTopic(string topicName);
    }
}