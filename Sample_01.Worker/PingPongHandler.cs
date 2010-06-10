using System.Diagnostics;
using System.Threading;
using CloudBus;
using Lokad.Quality;

namespace Sample_01.Worker
{
	[UsedImplicitly]
	public sealed class PingPongHandler : IConsumeMessage<PingPongCommand>
	{
		readonly IBusSender _sender;

		public PingPongHandler(IBusSender sender)
		{
			_sender = sender;
		}

		public void Consume(PingPongCommand message)
		{
			Trace.WriteLine("Ping " + message.Ball + " for game '" + message.Game +"'.");
			Thread.Sleep(1000);

			message.Ball += 1;
			_sender.Send(message);
		}
	}
}