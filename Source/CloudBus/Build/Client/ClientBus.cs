using System;
using System.Collections.Specialized;
using Autofac;
using Bus2.Queue;

namespace Bus2.Build.Client
{
	public class ClientBus : IClientBus
	{
		readonly IQueueManager _manager;
		readonly IContainer _container;

		readonly string _queueName;
		readonly IWriteMessageQueue _writeMessageQueue;

		public ClientBus(IQueueManager manager, IContainer container, string queueName)
		{
			_manager = manager;
			_container = container;
			_queueName = queueName;
			_writeMessageQueue = _manager.GetWriteQueue(queueName);
		}

		public void SendMessages(object[] messages, Action<NameValueCollection> headers)
		{
			_writeMessageQueue.SendMessages(messages, headers);
		}

		public void SendMessage(object message)
		{
			var end = new OutgoingMessageEnvelope
				{
					Topic = message.GetType().FullName,
					Sender = "Client"
				};

			_writeMessageQueue.SendMessages(new[] {message}, end.CopyHeaders);
		}
	}
}