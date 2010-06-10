#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using CloudBus.Queue;

namespace CloudBus.Sender
{
	sealed class DefaultBusSender : IBusSender
	{
		readonly IWriteMessageQueue _queue;

		public DefaultBusSender(IWriteMessageQueue queue)
		{
			_queue = queue;
		}

		public void Send(params IBusMessage[] messages)
		{
			if (messages.Length == 0)
				return;

			_queue.SendMessages(messages, nv => { });
		}
	}
}