namespace Lokad.Cqrs.Core.Transport
{
	public interface IReadQueue
	{
		string Name { get; }
		void Init();
		GetMessageResult GetMessage();

		/// <summary>
		/// ACKs the message by deleting it from the queue.
		/// </summary>
		/// <param name="message">The message context to ACK.</param>
		void AckMessage(MessageContext message);
	}
}