#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using Autofac;
using Lokad.Cqrs.Queue;
using Lokad.Quality;

namespace Lokad.Cqrs
{
	[UsedImplicitly]
	public class CloudClient : ICloudClient
	{
		readonly IQueueManager _manager;
		readonly IComponentContext _resolver;

		readonly IWriteMessageQueue _writeMessageQueue;

		public CloudClient(IQueueManager manager, string queueName, IComponentContext resolver)
		{
			_manager = manager;
			_resolver = resolver;
			_writeMessageQueue = _manager.GetWriteQueue(queueName);
		}

		public void SendMessage(object message)
		{
			_writeMessageQueue.SendMessages(new[] {message}, parts => parts.AddSender("Client"));
		}

		public TService Resolve<TService>()
		{
			return _resolver.Resolve<TService>();
		}

		public void SendMessages(object[] messages, Action<MessageAttributeBuilder> headers)
		{
			_writeMessageQueue.SendMessages(messages, headers);
		}
	}
}