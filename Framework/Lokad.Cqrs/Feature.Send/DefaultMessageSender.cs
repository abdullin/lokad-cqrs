#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Transactions;

namespace Lokad.Cqrs.Feature.Send
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

			if (Transaction.Current == null)
			{
				_queue.SendAsSingleMessage(new[] { message });
			}
			else
			{
				var action = new CommitActionEnlistment(() => _queue.SendAsSingleMessage(new[] { message }));
				Transaction.Current.EnlistVolatile(action, EnlistmentOptions.None);
			}
		}
		

		public void SendAsBatch(params object[] messageItems)
		{
			if (messageItems.Length == 0)
				return;

			if (Transaction.Current==null)
			{
				_queue.SendAsSingleMessage(messageItems);
			}
			else
			{
				var action = new CommitActionEnlistment(() => _queue.SendAsSingleMessage(messageItems));
				Transaction.Current.EnlistVolatile(action, EnlistmentOptions.None);
			}
		}
	}
}