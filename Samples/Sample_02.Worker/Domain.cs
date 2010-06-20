using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Lokad;
using Lokad.Cqrs;
using Lokad.Cqrs.Default;

namespace Sample_02.Worker
{
	[DataContract]
	public sealed class SendPaymentMessage : IMessage
	{
		[DataMember]
		public double Amount { get; private set; }

		public SendPaymentMessage(double amount)
		{
			Amount = amount;
		}

		public override string ToString()
		{
			// makes the message more readable in the debug logs
			return string.Format("Send Payment ({0})", Amount);
		}
	}

	public sealed class SendPaymentHandler : IConsume<SendPaymentMessage>
	{
		public void Consume(SendPaymentMessage message)
		{
			Trace.WriteLine("There is an incoming payment! " + message.Amount);
		}
	}

	public sealed class ListenToEverythingHandler : IConsume<IMessage>
	{
		public void Consume(IMessage message)
		{
			Trace.WriteLine("Got message of type: " + message.GetType());
		}
	}

	public sealed class SendPaymentsSometimes : IScheduledTask
	{
		readonly IMessageClient _sender;

		public SendPaymentsSometimes(IMessageClient sender)
		{
			// sender will be injected by the IoC
			_sender = sender;
		}

		public TimeSpan Execute()
		{
			var amount = (Rand.NextDouble()*100).Round(1);
			// send new message
			_sender.Send(new SendPaymentMessage(amount));
			// sleep for
			return 3.Seconds();
		}
	}
}