namespace Lokad.Cqrs.Core.Transport
{
	public interface IReadQueue
	{
		string Name { get; }
		void Init();
		GetMessageResult GetMessage();

		/// <summary>
		/// Acknowledges message consumption.
		/// </summary>
		/// <param name="message">The message context to ACK.</param>
		void AckMessage(MessageContext message);

		/// <summary>
		/// Sends negative acknowledgement to the queue in advance (i.e.: closing the lease).
		/// </summary>
		/// <param name="context">The message context to NACK.</param>
		void TryNotifyNack(MessageContext context);
	}
}