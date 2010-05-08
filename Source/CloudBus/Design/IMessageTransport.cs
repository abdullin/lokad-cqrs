using System;
using Bus2.Queue;

namespace Bus2
{
	public interface IMessageTransport : IDisposable
	{
		int ThreadCount { get; }
		void Start();

		event Action Started;
		event Func<IncomingMessage, bool> MessageRecieved;
	}
}