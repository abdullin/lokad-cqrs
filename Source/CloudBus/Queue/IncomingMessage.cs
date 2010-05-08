using System.Collections.Specialized;

namespace Bus2.Queue
{
	public sealed class IncomingMessage
	{
		public readonly object Message;
		public readonly string TransportMessageId;
		public readonly NameValueCollection Headers;
		public readonly string Topic;
		public readonly string Sender;


		public IncomingMessage(object message, IncomingMessageEnvelope envelope)
		{
			Message = message;
			TransportMessageId = envelope.TransportMessageId;
			Headers = envelope.Headers;

			Topic = envelope.Topic;
			Sender = envelope.Sender;
		}
	}
}