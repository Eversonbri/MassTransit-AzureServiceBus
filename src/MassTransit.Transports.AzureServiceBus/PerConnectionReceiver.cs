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
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using Receiver;
    using Util;


    /// <summary>
    /// Tiny wrapper around the F# receiver that is started only when the connection is
    /// bound and not before. The reason for this is that the receiver needs the management
    /// to do its work before becoming active. Possibly, the interface and hence implementation
    /// that calls the start and stop methods could be moved to the F# project.
    /// </summary>
    class PerConnectionReceiver
        : ConnectionBinding<AzureServiceBusConnection>
    {
        static readonly ILog _logger = Logger.Get(typeof(PerConnectionReceiver));
        readonly IAzureServiceBusEndpointAddress _address;

        readonly Action<Receiver.Receiver> _onBound;
        readonly Action<Receiver.Receiver> _onUnbound;
        readonly ReceiverSettings _settings;
        Receiver.Receiver _receiver;

        public PerConnectionReceiver(
            [NotNull] IAzureServiceBusEndpointAddress address,
            [NotNull] ReceiverSettings settings,
            [NotNull] Action<Receiver.Receiver> onBound,
            [NotNull] Action<Receiver.Receiver> onUnbound)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (onBound == null)
                throw new ArgumentNullException("onBound");
            if (onUnbound == null)
                throw new ArgumentNullException("onUnbound");

            _address = address;
            _settings = settings;
            _onBound = onBound;
            _onUnbound = onUnbound;
        }

        /// <summary>
        /// Normal Receiver is started
        /// </summary>
        public void Bind(AzureServiceBusConnection connection)
        {
            _logger.DebugFormat("starting receiver for '{0}'", _address);

            if (_receiver != null)
                return;

            _receiver = ReceiverModule.StartReceiver(_address, _settings);
            _receiver.Error += (sender, args) => _logger.Error("Error from receiver", args.Exception);
            _onBound(_receiver);
        }

        /// <summary>
        /// Normal Receiver is stopped/disposed
        /// </summary>
        public void Unbind(AzureServiceBusConnection connection)
        {
            _logger.DebugFormat("stopping receiver for '{0}'", _address);

            if (_receiver == null)
                return;

            _onUnbound(_receiver);

            ((IDisposable)_receiver).Dispose();
            _receiver = null;
        }

        /// <summary>
        /// Get a message from the queue (in memory one)
        /// </summary>
        public BrokeredMessage Get(TimeSpan timeout)
        {
            if (_receiver == null)
                throw new ApplicationException("Call Bind before calling Get");

            return _receiver.Get(timeout);
        }
    }
}