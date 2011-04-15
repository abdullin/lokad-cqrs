using System;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IWriteQueue
	{
		void PutMessage(MessageEnvelope envelope);
		void Init();
	}
}