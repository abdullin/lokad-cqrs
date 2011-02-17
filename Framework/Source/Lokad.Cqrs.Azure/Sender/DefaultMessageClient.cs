#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Queue;

namespace Lokad.Cqrs.Sender
{
	sealed class DefaultMessageClient : IMessageClient
	{
		readonly AzureMessageQueue _queue;

		public DefaultMessageClient(AzureMessageQueue queue)
		{
			_queue = queue;
		}

		public void Send(params object[] messages)
		{
			if (messages.Length == 0)
				return;

			_queue.SendMessages(messages, nv => { });
		}
	}
}