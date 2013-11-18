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
    using System.Collections.Generic;
    using Builders;
    using MassTransit.Configurators;


    public class NamespaceConnectionSettingsConfigurator :
        ConnectionSettingsConfigurator,
        AzureServiceBusTransportFactoryBuilderConfigurator
    {
        readonly string _namespace;
        string _key;
        string _keyName;

        public NamespaceConnectionSettingsConfigurator(string ns)
        {
            _namespace = ns;
        }

        public AzureServiceBusTransportFactoryBuilder Configure(AzureServiceBusTransportFactoryBuilder builder)
        {
            ConnectionSettingsBuilder connectionSettingsBuilder = Configure();

            builder.AddConnectionSettingsBuilder(_namespace, connectionSettingsBuilder);

            return builder;
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (string.IsNullOrEmpty(_key))
                yield return this.Failure("Key", "must not be null");
            if (string.IsNullOrEmpty(_keyName))
                yield return this.Failure("KeyName", "must not be null");
        }

        public void SetKeyName(string keyName)
        {
            _keyName = keyName;
        }

        public void SetKey(string key)
        {
            _key = key;
        }

        public ConnectionSettingsBuilder Configure()
        {
            return new SharedAccessSignatureConnectionSettingsBuilderImpl(_namespace, _keyName, _key);
        }
    }
}