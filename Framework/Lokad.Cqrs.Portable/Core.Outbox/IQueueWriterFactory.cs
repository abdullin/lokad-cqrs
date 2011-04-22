using System.Collections.Generic;

namespace Lokad.Cqrs
{
	public interface IQueueWriterFactory
	{
		bool TryGetWriteQueue(string queueName, out IQueueWriter writer);
	}


}