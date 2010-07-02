#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Queue;
using Lokad.Quality;

namespace Lokad.Cqrs.PubSub
{
	[UsedImplicitly]
	public sealed class PublishSubscribeProcess : IEngineProcess
	{
		readonly ILog _log;
		readonly IRouteMessages _router;
		readonly IPublishSubscribeManager _store;
		readonly IMessageTransport _transport;

		public PublishSubscribeProcess(IMessageTransport transport, IPublishSubscribeManager manager, IRouteMessages router,
			ILogProvider provider)
		{
			_log = provider.CreateLog<PublishSubscribeProcess>();
			_transport = transport;
			_store = manager;
			_router = router;
		}

		public void Dispose()
		{
			_log.DebugFormat("Stopping pub/sub for {0}", _transport.ToString());
			_transport.Dispose();
			_transport.MessageRecieved -= TransportOnMessageRecieved;
		}

		public void StartUp()
		{
			_log.DebugFormat("Starting pub/sub for {0}", _transport.ToString());
			_transport.MessageRecieved += TransportOnMessageRecieved;
			_transport.Start();
		}

		bool Manage(object message)
		{
			var direct = message as SubscribeDirectMessage;

			if (direct != null)
			{
				_log.DebugFormat("Subscribing '{0}' to '{1}'", direct.Queue, direct.Topic);
				_store.SubscribeDirect(
					direct.SubscriptionId,
					direct.Topic,
					direct.Queue);
				return true;
			}

			var regex = message as SubscribeRegexMessage;

			if (regex != null)
			{
				_log.DebugFormat("Subscribing '{0}' to '{1}'", regex.Queue, regex.Regex);
				_store.SubscribeRegex(
					regex.SubscriptionId,
					regex.Regex,
					regex.Queue);
				return true;
			}

			//_log.ErrorFormat("Unknown management message {0}", message);
			return false;
		}

		bool TransportOnMessageRecieved(UnpackedMessage incomingMessage)
		{
			if (Manage(incomingMessage.Content))
				return true;

			var topic = incomingMessage.Attributes.GetLastString(MessageAttributeType.Topic);

			if (!topic.HasValue)
			{
				_log.DebugFormat("Discarding message {0} without topic", incomingMessage.TransportMessageId);
				return false;
			}

			var subscribers = _store.GetSubscribers(topic.Value);
			if (subscribers.Length > 0)
			{
				_router.RouteMessages(new[] {incomingMessage}, subscribers);
				return true;
			}
			return false;
		}
	}
}