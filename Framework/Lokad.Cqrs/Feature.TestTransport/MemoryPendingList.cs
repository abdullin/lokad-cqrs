using System;
using System.Collections.Generic;
using System.Linq;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Core.Partition.Testable
{
	public sealed class MemoryPendingList
	{
		readonly IList<MessageEnvelope> _schedule = new List<MessageEnvelope>();
		readonly object _lock = new object();

		public void PutMessage(MessageEnvelope envelope)
		{
			lock (_lock)
			{
				
				_schedule.Add(envelope);
			}
		}

		public bool TakePendingMessage(out MessageEnvelope envelope)
		{
			var dateTimeOffset = DateTimeOffset.UtcNow;
			lock (_lock)
			{
				envelope = _schedule.OrderBy(x => x.DeliverOn).FirstOrDefault(t => t.DeliverOn <= dateTimeOffset);
				if (null != envelope)
				{
					_schedule.Remove(envelope);
					return true;
				}

			}
			return false;
		}
	}
}