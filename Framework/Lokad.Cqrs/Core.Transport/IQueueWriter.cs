using System;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IQueueWriter
	{
		void PutMessage(MessageEnvelope envelope);
		void Init();
	}
}