#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Concurrent;
using System.Threading;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryPartition : IPartitionScheduler
	{
		readonly BlockingCollection<MessageEnvelope>[] _queues;
		readonly string[] _names;

		public MemoryPartition(BlockingCollection<MessageEnvelope>[] queues, string[] names)
		{
			_queues = queues;
			_names = names;
		}

		public void Init()
		{
		}

		public void AckMessage(MessageContext message)
		{
			// do nothing
		}

		public bool TakeMessage(CancellationToken token, out MessageContext context)
		{
			MessageEnvelope envelope;
			var result = BlockingCollection<MessageEnvelope>.TakeFromAny(_queues, out envelope);
			if (result >= 0)
			{
				context = new MessageContext(result, envelope, _names[result]);
				return true;
			}
			context = null;
			return false;
		}

		public void TryNotifyNack(MessageContext context)
		{
			var id = (int) context.TransportMessage;

			_queues[id].Add(context.Unpacked);
		}
	}
}