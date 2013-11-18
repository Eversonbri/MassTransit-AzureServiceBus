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
    using System.Collections.Generic;
    using System.Linq;
    using Builders;
    using MassTransit.Configurators;


    public class AzureServiceBusFactoryConfiguratorImpl :
        AzureServiceBusTransportFactoryConfigurator,
        Configurator
    {
        readonly AzureServiceBusSettings _settings;
        readonly IList<AzureServiceBusTransportFactoryBuilderConfigurator> _transportFactoryConfigurators;

        public AzureServiceBusFactoryConfiguratorImpl()
        {
            _settings = new AzureServiceBusSettings();
            _transportFactoryConfigurators = new List<AzureServiceBusTransportFactoryBuilderConfigurator>();
        }

        void AzureServiceBusTransportFactoryConfigurator.SetLockDuration(TimeSpan lockDuration)
        {
            _settings.LockDuration = lockDuration;
        }

        void AzureServiceBusTransportFactoryConfigurator.SetDefaultMessageTimeToLive(TimeSpan ttl)
        {
            _settings.DefaultMessageTimeToLive = ttl;
        }

        void AzureServiceBusTransportFactoryConfigurator.SetDeadLetteringOnExpiration(bool enabled)
        {
            _settings.DeadLetteringOnExpiration = enabled;
        }

        void AzureServiceBusTransportFactoryConfigurator.SetBatchedOperations(bool enabled)
        {
            _settings.BatchedOperations = enabled;
        }

        void AzureServiceBusTransportFactoryConfigurator.SetReceiveTimeout(TimeSpan timeout)
        {
            _settings.ReceiveTimeout = timeout;
        }

        void AzureServiceBusTransportFactoryConfigurator.SetMaxOutstandingSendOperations(int number)
        {
            _settings.MaxOutstandingSendOperations = number;
        }

        public void AddConfigurator(AzureServiceBusTransportFactoryBuilderConfigurator configurator)
        {
            _transportFactoryConfigurators.Add(configurator);
        }

        void AzureServiceBusTransportFactoryConfigurator.SetReceiverName(string name)
        {
            _settings.ReceiverName = name;
        }

        public IEnumerable<ValidationResult> Validate()
        {
            if (_settings.MaxOutstandingSendOperations < 0)
                yield return this.Failure("MaxOutstandingSendOperations", "Must be >= 1");

            if (string.IsNullOrWhiteSpace(_settings.ReceiverName))
                yield return this.Failure("ReceiverName", "must not be empty");

            foreach (ValidationResult result in _transportFactoryConfigurators.SelectMany(x => x.Validate()))
                yield return result;
        }

        /// <summary>
        /// Configure and build the transport factory
        /// </summary>
        /// <returns></returns>
        public AzureServiceBusTransportFactory Configure()
        {
            AzureServiceBusTransportFactoryBuilder builder = new AzureServiceBusTransportFactoryBuilderImpl(_settings);

            builder = _transportFactoryConfigurators.Aggregate(builder,
                (current, configurator) => configurator.Configure(current));

            return builder.Build();
        }
    }
}