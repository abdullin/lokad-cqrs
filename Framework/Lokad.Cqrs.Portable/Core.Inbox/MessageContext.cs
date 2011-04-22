#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Core.Inbox
{
	public sealed class MessageContext
	{
		public readonly object TransportMessage;
		public readonly MessageEnvelope Unpacked;
		public readonly string QueueName;

		public MessageContext(object transportMessage, MessageEnvelope unpacked, string queueName)
		{
			TransportMessage = transportMessage;
			QueueName = queueName;
			Unpacked = unpacked;
		}
	}
}