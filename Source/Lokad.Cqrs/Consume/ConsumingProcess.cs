#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Queue;
using Lokad.Quality;

namespace Lokad.Cqrs.Consume
{
	[UsedImplicitly]
	public sealed class ConsumingProcess : IEngineProcess
	{
		readonly IMessageDispatcher _dispatcher;
		readonly ILog _log;
		readonly IMessageTransport _transport;

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