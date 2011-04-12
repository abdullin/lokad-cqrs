using Lokad.Cqrs.Core.Durability;

namespace Lokad.Cqrs.Feature.Send
{
	public interface IWriteQueue
	{
		void SendAsSingleMessage(object[] items);
		void ForwardMessage(MessageEnvelope envelope);
		void Init();
	}
}