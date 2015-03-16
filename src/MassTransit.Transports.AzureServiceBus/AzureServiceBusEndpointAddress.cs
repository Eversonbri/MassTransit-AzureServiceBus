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
    using System.Text.RegularExpressions;
    using Exceptions;
    using Magnum;
    using Magnum.Extensions;
    using Util;


    /// <summary>
    /// implemntation, see <see cref="IAzureServiceBusEndpointAddress"/>.
    /// </summary>
    public class AzureServiceBusEndpointAddress :
        IAzureServiceBusEndpointAddress
    {
        const string FormatErrorMsg =
            "The path can be empty, or a sequence of these characters: letters, digits, hyphen, underscore, or period.";

        static readonly Regex _regex = new Regex(@"^[A-Za-z0-9\-_\.]+$");

        readonly AddressType _addressType;
        readonly Uri _friendlyUri;
        readonly string _namespace;
        readonly int _prefetchCount;
        readonly string _queueOrTopicName;
        readonly IConnectionSettings _settings;


        AzureServiceBusEndpointAddress(AddressType addressType, string ns, string queueOrTopicName,
            IConnectionSettings settings, int prefetchCount)
        {
            _addressType = addressType;
            _namespace = ns;
            _queueOrTopicName = queueOrTopicName;
            _settings = settings;
            _prefetchCount = prefetchCount;

            string suffix = "";
            if (addressType == AddressType.Topic)
                suffix = "?topic=true";

            _friendlyUri = new Uri(string.Format("sb://{0}/{1}{2}",
                ns,
                queueOrTopicName,
                suffix));
        }

        public IConnectionSettings ConnectionSettings
        {
            get { return _settings; }
        }

        public string Namespace
        {
            get { return _namespace; }
        }

        public string TopicName
        {
            get
            {
                if (_addressType != AddressType.Topic)
                    throw new ArgumentException("Address is not a topic");

                return _queueOrTopicName;
            }
        }

        /// <summary>
        /// Return a new address for the specific topic name
        /// </summary>
        /// <param name="topicName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IAzureServiceBusEndpointAddress ForTopic(string topicName)
        {
            if (topicName == null)
                throw new ArgumentNullException("topicName");

            return new AzureServiceBusEndpointAddress(AddressType.Topic, _namespace, topicName, _settings,
                _prefetchCount);
        }

        /// <summary>
        /// This uri is MT-schemed, not AzureSB-schemed
        /// </summary>
        public Uri Uri
        {
            get { return _friendlyUri; }
        }

        bool IEndpointAddress.IsLocal
        {
            get { return false; }
        }

        bool IEndpointAddress.IsTransactional
        {
            get { return false; }
        }

        public string QueueName
        {
            get
            {
                if (_addressType != AddressType.Queue)
                    throw new EndpointException(Uri, "Address is not a queue");

                return _queueOrTopicName;
            }
        }

        public int PrefetchCount
        {
            get { return _prefetchCount; }
        }

        public TimeSpan DefaultMessageTimeToLive
        {
            get { return TimeSpan.MaxValue; }
        }

        public bool EnableBatchOperations
        {
            get { return true; }
        }

        public TimeSpan LockDuration
        {
            get { return TimeSpan.FromMinutes(5); }
        }

        public int MaxDeliveryCount
        {
            get { return 99; }
        }

        public bool IsQueue
        {
            get { return _addressType == AddressType.Queue; }
        }

        public bool IsTopic
        {
            get { return _addressType == AddressType.Topic; }
        }

        public override string ToString()
        {
            return _friendlyUri.ToString();
        }


        public static IAzureServiceBusEndpointAddress Parse(Uri address, IConnectionSettings connectionSettings = null)
        {
            Guard.AgainstNull(address, "address");

            if (string.Compare(Constants.Scheme, address.Scheme, StringComparison.OrdinalIgnoreCase) != 0)
                throw new EndpointException(address, "The invalid scheme was specified: " + address.Scheme);

            string name = address.AbsolutePath.TrimStart('/');

            var endpoint = new UriBuilder("sb", address.Host, 0, name).Uri;

            string keyName = null;
            string key = null;
             
            if (!address.UserInfo.IsEmpty())
            {
                if (address.UserInfo.Contains(":"))
                {
                    string[] parts = address.UserInfo.Split(':');
                    keyName = parts[0];
                    key = parts[1];
                }
            }

            string issuer = address.Query.GetValueFromQueryString("KeyName");
            if (!string.IsNullOrWhiteSpace(issuer))
                keyName = issuer;

            string value = address.Query.GetValueFromQueryString("Key");
            if (!string.IsNullOrWhiteSpace(value))
                key = value;

            bool topic = address.Query.GetValueFromQueryString("topic", false);
            AddressType addressType = topic
                                          ? AddressType.Topic
                                          : AddressType.Queue;

            VerifyQueueOrTopicNameAreLegal(address, name);

            int prefetchCount = address.Query.GetValueFromQueryString("prefetch", 100);

            string ns = address.Host;

            if (keyName == null && connectionSettings != null)
                keyName = connectionSettings.KeyName;

            if (key == null && connectionSettings != null)
                key = connectionSettings.Key;

            if (keyName == null || key == null)
                throw new ArgumentException("The Key or KeyName was not specified");

            var settings = new SharedAccessSignatureConnectionSettings(keyName, key);

            return new AzureServiceBusEndpointAddress(addressType, ns, name, settings, prefetchCount);
        }

        static void VerifyQueueOrTopicNameAreLegal(Uri address, string path)
        {
            Match match = _regex.Match(path);

            if (!match.Success)
                throw new EndpointException(address, FormatErrorMsg);
        }


        internal enum AddressType
        {
            Queue,
            Topic
        }
    }
}