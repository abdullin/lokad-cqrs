using System;

namespace Bus2.Queue
{
	public interface IReadMessageQueue
	{
		Uri Uri { get; }
		void Init();
		GetMessageResult GetMessage();
		void AckMessage(IncomingMessage message);
		void DiscardMessage(IncomingMessage message);
	}
}