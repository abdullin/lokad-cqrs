using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryReadQueue : IReadQueue
	{
		readonly ConcurrentQueue<MessageEnvelope> _queue;

		public MemoryReadQueue(ConcurrentQueue<MessageEnvelope> queue)
		{
			_queue = queue;
		}

		public void Init()
		{
		}

		public GetMessageResult TryGetMessage()
		{
			MessageEnvelope result;
			if (_queue.TryDequeue(out result))
			{
				return GetMessageResult.Success(new MessageContext(null, result));
			}
			return GetMessageResult.Wait;
		}

		public void AckMessage(MessageContext message)
		{
			
		}

		public void TryNotifyNack(MessageContext context)
		{
			_queue.Enqueue(context.Unpacked);
		}
	}
}