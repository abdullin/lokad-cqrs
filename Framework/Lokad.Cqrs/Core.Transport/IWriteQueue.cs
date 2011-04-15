using System;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IWriteQueue
	{
		void SendMessage(MessageEnvelope envelope);
		void Init();
	}
}