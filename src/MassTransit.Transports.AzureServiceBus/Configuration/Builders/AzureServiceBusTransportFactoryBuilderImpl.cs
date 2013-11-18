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
    using System.Collections.Generic;


    public class AzureServiceBusTransportFactoryBuilderImpl :
        AzureServiceBusTransportFactoryBuilder
    {
        readonly IDictionary<string, ConnectionSettingsBuilder> _connectionSettingsBuilders;
        readonly AzureServiceBusSettings _settings;

        public AzureServiceBusTransportFactoryBuilderImpl(AzureServiceBusSettings settings)
        {
            _settings = settings;
            _connectionSettingsBuilders = new Dictionary<string, ConnectionSettingsBuilder>();
        }

        public void AddConnectionSettingsBuilder(string ns, ConnectionSettingsBuilder connectionSettingsBuilder)
        {
            _connectionSettingsBuilders[ns] = connectionSettingsBuilder;
        }

        public AzureServiceBusTransportFactory Build()
        {
            var factory = new AzureServiceBusTransportFactory(_connectionSettingsBuilders.Values, _settings, _settings);

            return factory;
        }
    }
}