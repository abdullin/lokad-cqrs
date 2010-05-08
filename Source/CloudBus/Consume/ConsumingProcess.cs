using Bus2.Queue;
using Lokad;
using Lokad.Quality;

namespace Bus2.Consume
{
	[UsedImplicitly]
	public sealed class ConsumingProcess : IBusProcess
	{
		readonly IMessageTransport _transport;
		readonly ILog _log;
		readonly IMessageDispatcher _dispatcher;

		public ConsumingProcess(IMessageTransport transport, ILogProvider provider, IMessageDispatcher dispatcher)
		{
			_transport = transport;
			_dispatcher = dispatcher;
			_log = provider.CreateLog<ConsumingProcess>();
		}

		public void Dispose()
		{
			_log.DebugFormat("Stopping consumption for {0}", _transport.ToString());
			_transport.Dispose();
			_transport.MessageRecieved -= TransportOnMessageRecieved;
		}

		public void Start()
		{
			_log.DebugFormat("Starting consumption for {0}", _transport.ToString());
			_transport.MessageRecieved += TransportOnMessageRecieved;
			_transport.Start();
		}

		bool TransportOnMessageRecieved(IncomingMessage arg)
		{
			return _dispatcher.DispatchMessage(arg.Topic, arg.Message);
		}
	}
}