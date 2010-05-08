using System;
using System.Collections.Specialized;

namespace Bus2.Queue
{
	public interface IWriteMessageQueue
	{
		void SendMessages(object[] messages, Action<NameValueCollection> headers);
		void RouteMessages(IncomingMessage[] messages, Action<NameValueCollection> headers);
	}
}