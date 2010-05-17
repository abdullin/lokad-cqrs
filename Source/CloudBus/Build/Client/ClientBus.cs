#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using Autofac;
using CloudBus.Queue;
using Lokad.Quality;

namespace CloudBus.Build.Client
{
	[UsedImplicitly]
	public class ClientBus : IClientBus
	{
		readonly IQueueManager _manager;
		readonly IComponentContext _resolver;

		readonly IWriteMessageQueue _writeMessageQueue;

		public ClientBus(IQueueManager manager, string queueName, IComponentContext resolver)
		{
			_manager = manager;
			_resolver = resolver;
			_writeMessageQueue = _manager.GetWriteQueue(queueName);
		}

		public void SendMessage(object message)
		{
			var end = new OutgoingMessageEnvelope
				{
					Topic = message.GetType().FullName,
					Sender = "Client"
				};

			_writeMessageQueue.SendMessages(new[] {message}, end.CopyHeaders);
		}

		public TService Resolve<TService>()
		{
			return _resolver.Resolve<TService>();
		}

		public void SendMessages(object[] messages, Action<NameValueCollection> headers)
		{
			_writeMessageQueue.SendMessages(messages, headers);
		}
	}
}