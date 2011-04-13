using System.Collections.Concurrent;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Feature.TestTransport
{
	public sealed class MemoryReadQueue : IReadQueue
	{
		readonly ConcurrentQueue<MessageEnvelope> _queue;

		public string Name { get; private set; }

		public MemoryReadQueue(ConcurrentQueue<MessageEnvelope> queue, string name)
		{
			_queue = queue;
			Name = name;
		}

		public void Init()
		{
		}

		public GetMessageResult GetMessage()
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

	public sealed class MemoryWriteQueue : IWriteQueue
	{
		readonly ConcurrentQueue<MessageEnvelope> _queue;

		public MemoryWriteQueue(ConcurrentQueue<MessageEnvelope> queue)
		{
			_queue = queue;
		}

		public void SendMessage(MessageEnvelope envelope)
		{
			_queue.Enqueue(envelope);
		}

		public void Init()
		{
			
		}
	}
}