#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Transactions;
using Lokad.Cqrs.Core.Envelope;
using System.Linq;

namespace Lokad.Cqrs.Core.Outbox
{
	sealed class DefaultMessageSender : IMessageSender
	{
		readonly IQueueWriter _queue;
		readonly ISystemObserver _observer;
		readonly string _queueName;

		public DefaultMessageSender(IQueueWriter queue, ISystemObserver observer, string queueName)
		{
			_queue = queue;
			_observer = observer;
			_queueName = queueName;
		}

		public void Send(object message)
		{
			DelaySendBatch(default(TimeSpan), message);
		}

		public void DelaySend(TimeSpan timeout, object message)
		{
			DelaySendBatch(timeout, message);
		}

		public void SendBatch(params object[] messageItems)
		{
			DelaySendBatch(default(TimeSpan), messageItems);
		}

		public void DelaySendBatch(TimeSpan timeout, params object[] messageItems)
		{
			if (messageItems.Length == 0)
				return;

			var id = Guid.NewGuid().ToString().ToLowerInvariant();
			var builder = MessageEnvelopeBuilder.FromItems(id, messageItems);
			if (timeout != default(TimeSpan))
			{
				builder.DelayBy(timeout);
			}
			var envelope = builder.Build();

			if (Transaction.Current == null)
			{
				_queue.PutMessage(envelope);
				_observer.Notify(new EnvelopeSent(_queueName, envelope.EnvelopeId, false, envelope.Items.Select(x => x.MappedType.Name).ToArray()));
			}
			else
			{
				var action = new CommitActionEnlistment(() =>
					{
						_queue.PutMessage(envelope);
						_observer.Notify(new EnvelopeSent(_queueName, envelope.EnvelopeId, false, envelope.Items.Select(x => x.MappedType.Name).ToArray()));
					});
				Transaction.Current.EnlistVolatile(action, EnlistmentOptions.None);
			}
		}
	}
}