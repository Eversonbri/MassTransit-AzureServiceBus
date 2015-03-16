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
namespace MassTransit.Transports.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using Configuration.Builders;
    using Configuration.Configurators;
    using Exceptions;
    using Logging;
    using Magnum.Caching;
    using Util;


    /// <summary>
    ///     Implementation of the transport factory
    /// </summary>
    public class AzureServiceBusTransportFactory :
        ITransportFactory
    {
        static readonly ILog _log = Logger.Get(typeof(AzureServiceBusTransportFactory));

        readonly Cache<Uri, IAzureServiceBusEndpointAddress> _addresses;
        readonly Cache<string, IConnectionSettings> _connectionSettings;
        readonly Cache<string, ConnectionSettingsBuilder> _connectionSettingsBuilders;
        readonly Cache<string, ConnectionHandler<AzureServiceBusConnection>> _connections;
        readonly AzureServiceBusMessageNameFormatter _formatter;

        readonly IInboundSettings _inboundSettings;
        readonly IOutboundSettings _outboundSettings;
        bool _disposed;

        public AzureServiceBusTransportFactory(IEnumerable<ConnectionSettingsBuilder> builders,
            IInboundSettings inboundSettings, IOutboundSettings outboundSettings)
        {
            _addresses = new ConcurrentCache<Uri, IAzureServiceBusEndpointAddress>();
            _connections = new ConcurrentCache<string, ConnectionHandler<AzureServiceBusConnection>>();
            _connectionSettings = new ConcurrentCache<string, IConnectionSettings>(StringComparer.InvariantCultureIgnoreCase);
            _connectionSettingsBuilders = new ConcurrentCache<string, ConnectionSettingsBuilder>(x => x.Namespace, builders);

            _formatter = new AzureServiceBusMessageNameFormatter();

            _inboundSettings = inboundSettings;
            _outboundSettings = outboundSettings;

            _log.Debug("created new transport factory");
        }

        public IEndpointAddress GetAddress(Uri uri, bool transactional)
        {
            return _addresses.Get(uri, _ => AzureServiceBusEndpointAddress.Parse(uri, GetConnectionSettings(uri.Host)));
        }

        /// <summary>
        ///     Gets the scheme. (af-queues)
        /// </summary>
        public string Scheme
        {
            get { return Constants.Scheme; }
        }

        /// <summary>
        ///     The message name formatter associated with this transport
        /// </summary>
        public IMessageNameFormatter MessageNameFormatter
        {
            get { return _formatter; }
        }

        /// <summary>
        ///     Builds the duplex transport.
        /// </summary>
        /// <param name="settings"> The settings. </param>
        /// <returns> </returns>
        public IDuplexTransport BuildLoopback([NotNull] ITransportSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            _log.Debug("building duplex transport");

            return new Transport(settings.Address, () => BuildInbound(settings), () => BuildOutbound(settings));
        }

        /// <summary>
        ///     Builds the inbound transport for the service bus endpoint,
        /// </summary>
        /// <param name="settings"> using these settings </param>
        /// <returns> A non-null instance of the inbound transport. </returns>
        public virtual IInboundTransport BuildInbound([NotNull] ITransportSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            Uri uri = settings.Address.Uri;
            EnsureProtocolIsCorrect(uri);
            IAzureServiceBusEndpointAddress address = _addresses.Get(uri,
                key => AzureServiceBusEndpointAddress.Parse(uri, GetConnectionSettings(uri.Host)));

            _log.DebugFormat("building inbound transport for address '{0}'", address);

            ConnectionHandler<AzureServiceBusConnection> connectionHandler = GetConnection(_connections, address);

            return new InboundAzureServiceBusTransport(address, connectionHandler,
                MessageNameFormatter, _inboundSettings);
        }

        /// <summary>
        ///     Builds the outbound transport
        /// </summary>
        /// <param name="settings"> with settings </param>
        /// <returns> The outbound transport instance, non-null </returns>
        public virtual IOutboundTransport BuildOutbound([NotNull] ITransportSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            Uri uri = settings.Address.Uri;
            EnsureProtocolIsCorrect(uri);
            IAzureServiceBusEndpointAddress address = _addresses.Get(uri,
                key => AzureServiceBusEndpointAddress.Parse(uri, GetConnectionSettings(uri.Host)));

            _log.DebugFormat("building outbound transport for address '{0}'", address);

            ConnectionHandler<AzureServiceBusConnection> connectionHandler = GetConnection(_connections, address);

            return new OutboundAzureServiceBusTransport(address, connectionHandler, _outboundSettings);
        }

        /// <summary>
        ///     Builds the outbound error transport; where to send messages that fail.
        /// </summary>
        /// <param name="settings"> using these settings </param>
        /// <returns> The outbound transport instance, non null </returns>
        public virtual IOutboundTransport BuildError([NotNull] ITransportSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException("settings");

            Uri uri = settings.Address.Uri;
            EnsureProtocolIsCorrect(uri);
            IAzureServiceBusEndpointAddress address = _addresses.Get(uri,
                key => AzureServiceBusEndpointAddress.Parse(uri, GetConnectionSettings(uri.Host)));

            _log.DebugFormat("building error transport for address '{0}'", address);

            ConnectionHandler<AzureServiceBusConnection> connectionHandler = GetConnection(_connections, address);
            return new OutboundAzureServiceBusTransport(address, connectionHandler, _outboundSettings);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _connections.Each(x => x.Dispose());
            _connections.Clear();

            _addresses.Clear();

            _disposed = true;
        }


        IConnectionSettings GetConnectionSettings(string ns)
        {
            return _connectionSettings.Get(ns, _ =>
                {
                    var builder = _connectionSettingsBuilders.Get(ns, __ =>
                        {
                            throw new ArgumentException("Unable to get the settings for " + ns);
                        });

                    return builder.Build();
                });
        }

        /// <summary>
        ///     Ensures the protocol is correct.
        /// </summary>
        /// <param name="address"> The address. </param>
        void EnsureProtocolIsCorrect([NotNull] Uri address)
        {
            if (address == null)
                throw new ArgumentNullException("address");
            if (address.Scheme != Scheme)
            {
                throw new EndpointException(address,
                    string.Format("Address must start with '{1}' not '{0}'", address.Scheme, Scheme));
            }
        }


        ConnectionHandler<AzureServiceBusConnection> GetConnection(
            Cache<string, ConnectionHandler<AzureServiceBusConnection>> cache,
            IAzureServiceBusEndpointAddress address)
        {
            var ns = address.Uri.Host;

            return cache.Get(ns, _ =>
                {
                    if (_log.IsDebugEnabled)
                        _log.DebugFormat("Creating Azure Service Bus connection: {0}", address.Uri);

                    ConnectionSettingsBuilder builder = _connectionSettingsBuilders.Get(ns, __ =>
                        {
                            if (_log.IsDebugEnabled)
                                _log.DebugFormat("Using default configurator for connection: {0}", address.Uri);

                            var configurator = new NamespaceConnectionSettingsConfigurator(ns);

                            return configurator.Configure();
                        });

                    IConnectionSettings connectionSettings = builder.Build();

                    var connection = new AzureServiceBusConnectionImpl(address, connectionSettings.TokenProvider);

                    return new ConnectionHandlerImpl<AzureServiceBusConnection>(connection);
                });
        }
    }
}