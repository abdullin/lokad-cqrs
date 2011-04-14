using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IPartitionSchedulerFactory
	{
		IPartitionScheduler GetNotifier(string[] queueNames);
	}
}