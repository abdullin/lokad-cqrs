using System.Threading;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IPartitionScheduler
	{
		void Init();
		void AckMessage(MessageContext message);
		bool TakeMessage(CancellationToken token, out MessageContext context);
		void TryNotifyNack(MessageContext context);
	}
}