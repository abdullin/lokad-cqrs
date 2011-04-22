namespace Lokad.Cqrs
{
	public interface IQueueWriter
	{
		void PutMessage(MessageEnvelope envelope);
		void Init();
	}
}