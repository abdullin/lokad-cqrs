using System.Collections.Generic;
using System.Threading;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IPartitionFactory
	{
		IPartitionNotifier GetNotifier(string[] queueNames);
	}

	public interface IPartitionNotifier
	{
		void Init();
		void AckMessage(MessageContext message);
		bool TakeMessage(CancellationToken token, out MessageContext context);
		void TryNotifyNack(MessageContext context);
	}


}