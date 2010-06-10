using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using CloudBus;
using Lokad.Quality;

namespace Sample_01.Worker
{
	[DataContract]
	public sealed class PingPongCommand : IBusMessage
	{
		[DataMember]
		public int Ball { get; private set; }
		[DataMember]
		public string Game { get; private set; }

		public PingPongCommand(int ball, string game)
		{
			Ball = ball;
			Game = game;
		}

		public PingPongCommand Pong()
		{
			return new PingPongCommand(Ball+1, Game);
		}
	}

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

			
			_sender.Send(message.Pong());
		}
	}
}