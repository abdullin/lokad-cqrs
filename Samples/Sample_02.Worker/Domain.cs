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
			Trace.WriteLine("Generic listener just got message of type: " + message.GetType());
		}
	}

	public sealed class SendPaymentsSometimes : IScheduledTask
	{
		readonly IMessageClient _sender;

		public SendPaymentsSometimes(IMessageClient sender)
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