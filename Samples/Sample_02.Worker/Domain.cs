using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using CloudBus;
using Lokad;

namespace Sample_02.Worker
{
	[DataContract]
	public sealed class SendPaymentMessage : IBusMessage
	{
		[DataMember]
		public double Amount { get; private set; }

		public SendPaymentMessage(double amount)
		{
			Amount = amount;
		}
	}

	public sealed class SendPaymentHandler : IConsumeMessage<SendPaymentMessage>
	{
		public void Consume(SendPaymentMessage message)
		{
			Trace.WriteLine("There is an incoming payment! " + message.Amount);
		}
	}

	public sealed class ListenToEverythingHandler : IConsumeMessage<IBusMessage>
	{
		public void Consume(IBusMessage message)
		{
			Trace.WriteLine("Generic listener just got message of type: " + message.GetType());
		}
	}

	public sealed class SendPaymentsSometimes : IBusTask
	{
		readonly IBusSender _sender;

		public SendPaymentsSometimes(IBusSender sender)
		{
			_sender = sender;
		}

		public TimeSpan Execute()
		{
			var amount = (Rand.NextDouble()*100).Round(1);
			_sender.Send(new SendPaymentMessage(amount));
			// sleep for
			return 3.Seconds();
		}
	}
}