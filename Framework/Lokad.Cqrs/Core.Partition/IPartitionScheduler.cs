using System.Threading;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Core.Partition
{
	public interface IPartitionScheduler
	{
		void Init();
		void AckMessage(MessageContext message);
		bool TakeMessage(CancellationToken token, out MessageContext context);
		void TryNotifyNack(MessageContext context);
	}
}