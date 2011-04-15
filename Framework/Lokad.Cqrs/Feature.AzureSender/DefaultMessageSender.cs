#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Transactions;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.AzureSender
{
	sealed class DefaultMessageSender : IMessageSender
	{
		readonly IWriteQueue _queue;

		public DefaultMessageSender(IWriteQueue queue)
		{
			_queue = queue;
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
				_queue.SendMessage(envelope);
			}
			else
			{
				var action = new CommitActionEnlistment(() => _queue.SendMessage(envelope));
				Transaction.Current.EnlistVolatile(action, EnlistmentOptions.None);
			}
		}

	}
}