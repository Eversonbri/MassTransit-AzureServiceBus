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
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.ServiceBus.Messaging;


    public class Publisher :
        ConnectionBinding<AzureServiceBusConnection>
    {
        readonly IAzureServiceBusEndpointAddress _address;
        readonly HashSet<SubscriptionDescription> _subscriptions;
        readonly HashSet<string> _topics;
        AzureServiceBusConnection _connection;

        public Publisher(IAzureServiceBusEndpointAddress address)
        {
            _address = address;
            _subscriptions = new HashSet<SubscriptionDescription>(new SubscriptionDescriptionEqualityComparer());
            _topics = new HashSet<string>();
        }


        void ConnectionBinding<AzureServiceBusConnection>.Bind(AzureServiceBusConnection connection)
        {
            Interlocked.Exchange(ref _connection, connection);

            RebindExchanges();
        }

        void ConnectionBinding<AzureServiceBusConnection>.Unbind(AzureServiceBusConnection connection)
        {
            Interlocked.Exchange(ref _connection, null);
        }

        public void CreateTopic(string name)
        {
            lock(_topics)
                _topics.Add(name);

            try
            {
                if (_connection != null)
                    _connection.CreateTopic(name);
            }
            catch (MessagingEntityAlreadyExistsException ex)
            {
            }
        }

        public void AddTopicSubscription(string destination, string source)
        {
            string name = string.Format("{0}{1}", destination, source.GetHashCode());

            var description = new SubscriptionDescription(source, name)
            {
                ForwardTo = destination,
            };

            lock(_subscriptions)
                _subscriptions.Add(description);

            CreateSubscription(description);
        }

        void CreateSubscription(SubscriptionDescription description)
        {
            try
            {
                if (_connection != null)
                    _connection.NamespaceManager.CreateSubscription(description);
            }
            catch (MessagingEntityAlreadyExistsException ex)
            {
            }
        }

        public void RemoveTopicSubscription(string destination, string source)
        {
            var description = new SubscriptionDescription(source, source + "--" + destination)
            {
                ForwardTo = destination,
            };

            lock(_subscriptions)
                _subscriptions.Add(description);

            try
            {
                if (_connection != null)
                    _connection.NamespaceManager.DeleteSubscription(description.TopicPath, description.Name);
            }
            catch (MessagingEntityNotFoundException ex)
            {
            }
        }

        void RebindExchanges()
        {
            lock(_subscriptions)
            {
                if (_connection != null)
                {
                    _connection.CreateQueue(_address.QueueName);

                    IEnumerable<string> topics = _subscriptions.Select(x => x.ForwardTo)
                        .Concat(_subscriptions.Select(x => x.TopicPath))
                        .Concat(_topics)
                        .Where(x => x != _address.QueueName)
                        .Distinct();

                    foreach (string topic in topics)
                        _connection.CreateTopic(topic);

                    foreach (SubscriptionDescription subscription in _subscriptions)
                        CreateSubscription(subscription);
                }
            }
        }


        class SubscriptionDescriptionEqualityComparer :
            IEqualityComparer<SubscriptionDescription>
        {
            public bool Equals(SubscriptionDescription x, SubscriptionDescription y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (ReferenceEquals(x, null))
                    return false;
                if (ReferenceEquals(y, null))
                    return false;
                if (x.GetType() != y.GetType())
                    return false;
                return string.Equals(x.TopicPath, y.TopicPath) && string.Equals(x.ForwardTo, y.ForwardTo);
            }

            public int GetHashCode(SubscriptionDescription obj)
            {
                unchecked
                {
                    return ((obj.TopicPath != null
                        ? obj.TopicPath.GetHashCode()
                        : 0) * 397) ^ (obj.ForwardTo != null
                            ? obj.ForwardTo.GetHashCode()
                            : 0);
                }
            }
        }


        class TopicDescriptionEqualityComparer :
            IEqualityComparer<TopicDescription>
        {
            public bool Equals(TopicDescription x, TopicDescription y)
            {
                return x.Path.Equals(y.Path);
            }

            public int GetHashCode(TopicDescription obj)
            {
                return obj.Path != null
                    ? obj.Path.GetHashCode()
                    : 0;
            }
        }
    }
}