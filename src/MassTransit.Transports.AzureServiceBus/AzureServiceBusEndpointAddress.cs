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

#pragma warning disable 1591

namespace MassTransit.Transports.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Configurators;
    using Magnum.Extensions;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Receiver;
    using Util;


    /// <summary>
    /// implemntation, see <see cref="IAzureServiceBusEndpointAddress"/>.
    /// </summary>
    public class AzureServiceBusEndpointAddress
        : IAzureServiceBusEndpointAddress
    {
        readonly Data _data;
        readonly Uri _friendlyUri;
        readonly Func<MessagingFactory> _mff;
        readonly NamespaceManager _nm;

        readonly QueueDescription _queueDescription;
        readonly Uri _rebuiltUri;
        readonly TopicDescription _topicDescription;
        readonly TokenProvider _tp;


        AzureServiceBusEndpointAddress([NotNull] Data data,
            AddressType addressType)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            _data = data;

            _tp = TokenProvider.CreateSharedSecretTokenProvider(_data.UsernameIssuer,
                _data.PasswordSharedSecret);

            Uri sbUri = ServiceBusEnvironment.CreateServiceUri("sb", _data.Namespace, string.Empty);

            var mfs = new MessagingFactorySettings
                {
                    TokenProvider = _tp,
                    NetMessagingTransportSettings =
                        {
                            // todo: configuration setting
                            BatchFlushInterval = 50.Milliseconds()
                        },
                    OperationTimeout = 3.Seconds()
                };
            _mff = () => MessagingFactory.Create(sbUri, mfs);

            _nm = new NamespaceManager(sbUri, _tp);

            string suffix = "";
            if (addressType == AddressType.Queue)
                _queueDescription = new QueueDescriptionImpl(data.QueueOrTopicName);
            else
            {
                _topicDescription = new TopicDescriptionImpl(data.QueueOrTopicName);
                suffix = "?topic=true";
            }

            _rebuiltUri = new Uri(string.Format("azure-sb://{0}:{1}@{2}/{3}{4}",
                data.UsernameIssuer,
                Uri.EscapeDataString(data.PasswordSharedSecret),
                data.Namespace,
                data.QueueOrTopicName,
                suffix));

            _friendlyUri = new Uri(string.Format("azure-sb://{0}/{1}{2}",
                data.Namespace,
                data.QueueOrTopicName,
                suffix));
        }

        [NotNull]
        internal Data Details
        {
            get { return _data; }
        }

        public TokenProvider TokenProvider
        {
            get { return _tp; }
        }

        public Func<MessagingFactory> MessagingFactoryFactory
        {
            get { return _mff; }
        }

        public NamespaceManager NamespaceManager
        {
            get { return _nm; }
        }

        public Task CreateQueue()
        {
            if (QueueDescription == null)
                throw new InvalidOperationException(
                    "Cannot create queue is the endpoint address is not for a queue (but for a topic)");

            return _nm.CreateAsync(QueueDescription);
        }

        public QueueDescription QueueDescription
        {
            get { return _queueDescription; }
        }

        public TopicDescription TopicDescription
        {
            get { return _topicDescription; }
        }

        public IAzureServiceBusEndpointAddress ForTopic(string topicName)
        {
            if (topicName == null)
                throw new ArgumentNullException("topicName");

            return new AzureServiceBusEndpointAddress(new Data
                {
                    AddressType = AddressType.Topic,
                    Namespace = _data.Namespace,
                    PasswordSharedSecret = _data.PasswordSharedSecret,
                    UsernameIssuer = _data.UsernameIssuer,
                    QueueOrTopicName = topicName,
                }, AddressType.Topic);
        }

        /// <summary>
        /// This uri is MT-schemed, not AzureSB-schemed
        /// </summary>
        public Uri Uri
        {
            get { return _rebuiltUri; }
        }

        bool IEndpointAddress.IsLocal
        {
            get { return false; }
        }

        bool IEndpointAddress.IsTransactional
        {
            get { return false; }
        }

        public void Dispose()
        {
        }

        public override string ToString()
        {
            return _friendlyUri.ToString();
        }

        // factory methods:

        public static AzureServiceBusEndpointAddress Parse([NotNull] Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            Data data;
            IEnumerable<ValidationResult> results;
            return TryParseInternal(uri, out data, out results)
                       ? new AzureServiceBusEndpointAddress(data, data.AddressType)
                       : ParseFailed(uri, results);
        }

        public static bool TryParse([NotNull] Uri inputUri, out AzureServiceBusEndpointAddress address,
            out IEnumerable<ValidationResult> validationResults)
        {
            if (inputUri == null)
                throw new ArgumentNullException("inputUri");
            Data data;
            if (TryParseInternal(inputUri, out data, out validationResults))
            {
                address = new AzureServiceBusEndpointAddress(data, data.AddressType);
                return true;
            }
            address = null;
            return false;
        }

        static AzureServiceBusEndpointAddress ParseFailed(Uri uri, IEnumerable<ValidationResult> results)
        {
            throw new ArgumentException(
                string.Format("There were problems parsing the uri '{0}': ", uri)
                + string.Join(", ", results));
        }

        static bool TryParseInternal(Uri uri, out Data data, out IEnumerable<ValidationResult> results)
        {
            data = null;
            var res = new List<ValidationResult>();

            if (string.IsNullOrWhiteSpace(uri.UserInfo) || !uri.UserInfo.Contains(":"))
            {
                res.Add(new ValidationResultImpl(ValidationResultDisposition.Failure, "UserInfo",
                    "UserInfo part of uri (stuff before @-character), doesn't exist or doesn't " +
                    "contain the :-character."));
                results = res;
                return false;
            }

            if (uri.AbsolutePath.LastIndexOf('/') != 0) // first item must be /
            {
                res.Add(new ValidationResultImpl(ValidationResultDisposition.Failure, "Application",
                    "AbsolutePath part of uri (stuff after hostname), contains more than one slash"));
                results = res;
                return false;
            }

            data = new Data
                {
                    UsernameIssuer = uri.UserInfo.Split(':')[0],
                    PasswordSharedSecret = Uri.UnescapeDataString(uri.UserInfo.Split(':')[1]),
                    Namespace = uri.Host.Contains(".")
                                    ? uri.Host.Substring(0, uri.Host.IndexOf('.'))
                                    : uri.Host,
                    QueueOrTopicName = uri.AbsolutePath.TrimStart('/'),
                    AddressType = uri.PathAndQuery.Contains("topic=true")
                                      ? AddressType.Topic
                                      : AddressType.Queue
                };

            results = null;
            return true;
        }


        internal enum AddressType
        {
            Queue,
            Topic
        }


        internal class Data
        {
            public string UsernameIssuer { get; set; }
            public string PasswordSharedSecret { get; set; }
            public string Namespace { get; set; }
            public string QueueOrTopicName { get; set; }
            public AddressType AddressType { get; set; }
        }
    }
}