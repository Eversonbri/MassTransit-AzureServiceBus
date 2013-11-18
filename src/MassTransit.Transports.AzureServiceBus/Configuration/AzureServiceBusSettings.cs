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
namespace MassTransit.Transports.AzureServiceBus.Configuration
{
    using System;


    public class AzureServiceBusSettings :
        IInboundSettings,
        IOutboundSettings
    {
        public TimeSpan LockDuration { get; set; }

        public TimeSpan DefaultMessageTimeToLive { get; set; }

        public bool DeadLetteringOnExpiration { get; set; }

        public bool BatchedOperations { get; set; }

        public TimeSpan ReceiveTimeout { get; set; }

        public int MaxOutstandingSendOperations { get; set; }

        public string ReceiverName { get; set; }
    }
}