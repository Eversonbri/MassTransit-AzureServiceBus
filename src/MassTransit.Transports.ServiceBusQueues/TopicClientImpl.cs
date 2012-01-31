using System;
using System.Threading.Tasks;
using MassTransit.Transports.ServiceBusQueues.Internal;
using MassTransit.Util;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using log4net;
using SBSubDesc = Microsoft.ServiceBus.Messaging.SubscriptionDescription;

namespace MassTransit.Transports.ServiceBusQueues
{
	public delegate Task UnsubscribeAction();

	// handles subscribers
	public class TopicClientImpl : TopicClient
	{
		static readonly ILog _logger = LogManager.GetLogger(typeof (TopicClientImpl));
		
		readonly MessagingFactory _messagingFactory;
		readonly NamespaceManager _namespaceManager;
		bool _isDisposed;
		Func<string, Microsoft.ServiceBus.Messaging.TopicClient> _clientFac;

		public TopicClientImpl(
			[NotNull] MessagingFactory messagingFactory,
			[NotNull] NamespaceManager namespaceManager)
		{
			if (messagingFactory == null) throw new ArgumentNullException("messagingFactory");
			if (namespaceManager == null) throw new ArgumentNullException("namespaceManager");

			_messagingFactory = messagingFactory;
			_namespaceManager = namespaceManager;
			_clientFac = path => _messagingFactory.CreateTopicClient(path);
		}

		public Task Send(BrokeredMessage msg, Topic topic)
		{
			_logger.Debug("being send");
			// todo: client has Close method... but not dispose.
			var client = _clientFac(topic.Description.Path);
			return Task.Factory.FromAsync(client.BeginSend, client.EndSend, msg, null)
				.ContinueWith(tSend => _logger.Debug("end send"));
		}

		public Task<Tuple<UnsubscribeAction, Subscriber>> Subscribe([NotNull] Topic topic,
			SubscriptionDescription description,
			ReceiveMode mode,
			string subscriberName)
		{
			if (topic == null) throw new ArgumentNullException("topic");

			description = description ?? new SubscriptionDescriptionImpl(topic.Description.Path, subscriberName);
			
			// Missing: no mf.BeginCreateSubscriptionClient?
			_logger.Debug(string.Format("being create subscription @ {0}", description));
			return Task.Factory
				.FromAsync<SBSubDesc, SBSubDesc>(
					_namespaceManager.BeginCreateSubscription,
					_namespaceManager.EndCreateSubscription,
					description.IDareYou, null)
				//.Then(tSbSubDesc => Task.Factory.FromAsync<string, ReceiveMode, MessageReceiver>(
				//    mf.BeginCreateMessageReceiver, mf.EndCreateMessageReceiver,
				//    description.TopicPath, mode, /* state */ _namespaceManager))
				.ContinueWith(tSubDesc => _messagingFactory.CreateSubscriptionClient(description.TopicPath, description.Name, mode))
				.ContinueWith(tMsgR =>
					{
						_logger.Debug(string.Format("end create message receiver @ {0}", description));
						var nm = _namespaceManager;
						return Tuple.Create(
							new UnsubscribeAction(() =>
								{
									_logger.Debug(string.Format("begin delete subscription @ {0}", description));
									return Task.Factory
										.FromAsync(nm.BeginDeleteSubscription, nm.EndDeleteSubscription,
											description.TopicPath, description.Name, null)
										.ContinueWith(tDeleteSub => 
											_logger.Debug(string.Format("end delete subscription @ {0}", description)));
								}), 
							new SubscriberImpl(tMsgR.Result) as Subscriber);
					});
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void Dispose(bool managed)
		{
			if (!managed) return;

			_logger.Debug("dispose called");

			if (_isDisposed)
				throw new ObjectDisposedException("TopicClientImpl", "cannot dispose twice");

			_isDisposed = true;
		}
	}
}