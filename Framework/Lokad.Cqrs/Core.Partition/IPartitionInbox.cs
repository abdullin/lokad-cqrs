using System.Threading;
using Lokad.Cqrs.Core.Transport;

namespace Lokad.Cqrs.Core.Partition
{
	public interface IPartitionInbox
	{
		void Init();
		void AckMessage(MessageContext message);
		bool TakeMessage(CancellationToken token, out MessageContext context);
		void TryNotifyNack(MessageContext context);
	}
}