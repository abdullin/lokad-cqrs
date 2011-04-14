using System.Threading;

namespace Lokad.Cqrs.Core.Transport
{
	public interface IReadQueue
	{
		void Init();
		GetMessageResult TryGetMessage();

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