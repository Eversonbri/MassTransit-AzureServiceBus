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
    using Logging;
    using Microsoft.ServiceBus.Messaging;
    using Subscriptions.Coordinator;
    using Subscriptions.Messages;
    using Util;


    /// <summary>
    /// 	Monitors the subscriptions from the local bus and subscribes the topics with topic clients when subscriptions occur: when they do; create the appropriate topics for them.
    /// </summary>
    public class TopicSubscriptionObserver :
        SubscriptionObserver
    {
        static readonly ILog _log = Logger.Get(typeof(TopicSubscriptionObserver));
        readonly IDictionary<Guid, TopicDescription> _bindings;

        readonly IMessageNameFormatter _formatter;
        readonly InboundAzureServiceBusTransport _inboundTransport;

        public TopicSubscriptionObserver([NotNull] InboundAzureServiceBusTransport inboundTransport, [NotNull] IMessageNameFormatter formatter)
        {
            if (formatter == null)
                throw new ArgumentNullException("formatter");
            if (inboundTransport == null)
                throw new ArgumentNullException("inboundTransport");

            _formatter = formatter;
            _inboundTransport = inboundTransport;

            _bindings = new Dictionary<Guid, TopicDescription>();
        }

        public void OnSubscriptionAdded(SubscriptionAdded message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            Type messageType = Type.GetType(message.MessageName);
            if (messageType == null)
            {
                _log.InfoFormat("Unknown message type '{0}', unable to add subscription", message.MessageName);
                return;
            }

            MessageName messageName = _formatter.GetMessageName(messageType);
            var topicDescription = new TopicDescription(messageName.ToString());

            _inboundTransport.AddTopicSubscriber(topicDescription);

            _bindings[message.SubscriptionId] = topicDescription;
        }

        public void OnSubscriptionRemoved(SubscriptionRemoved message)
        {
            _log.Debug(string.Format("subscription removed: '{0}'", message));

            TopicDescription topicDescription;
            if (_bindings.TryGetValue(message.SubscriptionId, out topicDescription))
            {
                _inboundTransport.RemoveTopicSubscriber(topicDescription);

                _bindings.Remove(message.SubscriptionId);
            }
        }

        public void OnComplete()
        {
        }
    }
}