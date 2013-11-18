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
namespace MassTransit.Transports.AzureServiceBus.Configuration.Builders
{
    public interface ConnectionSettingsBuilder
    {
        /// <summary>
        /// The namespace for the connection settings builder
        /// </summary>
        string Namespace { get; }

        /// <summary>
        ///     Build the connection settings, which are used to create a connection to the Azure ServiceBus
        /// </summary>
        /// <returns></returns>
        IConnectionSettings Build();
    }
}