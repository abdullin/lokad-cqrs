namespace Lokad.Cqrs.Core.Transport
{
	public interface IWriteQueue
	{
		//void SendAsSingleMessage(object[] items);
		//void ForwardMessage(MessageEnvelope envelope);


		void SendMessage(MessageEnvelope envelope);
		void Init();
	}
}