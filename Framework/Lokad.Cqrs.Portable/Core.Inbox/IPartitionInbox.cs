using System.Threading;

namespace Lokad.Cqrs.Core.Inbox
{
	public interface IPartitionInbox
	{
		void Init();
		void AckMessage(MessageContext message);
		bool TakeMessage(CancellationToken token, out MessageContext context);
		void TryNotifyNack(MessageContext context);
	}
}