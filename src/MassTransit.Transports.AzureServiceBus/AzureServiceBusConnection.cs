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
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;


    public interface AzureServiceBusConnection :
        Connection
    {
        /// <summary>
        /// The messaging factory for the connection
        /// </summary>
        MessagingFactory MessagingFactory { get; }

        /// <summary>
        /// The namespace manager for the connection
        /// </summary>
        NamespaceManager NamespaceManager { get; }
    }
}