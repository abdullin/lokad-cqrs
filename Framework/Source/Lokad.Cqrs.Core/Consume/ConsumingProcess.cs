#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Threading;
using System.Threading.Tasks;

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

		public void Initialize()
		{
			_transport.MessageReceived += TransportOnMessageRecieved;
			_transport.Initialize();
		}

		public void Dispose()
		{
			_transport.MessageReceived -= TransportOnMessageRecieved;
			_transport.Dispose();
		}

		public Task Start(CancellationToken token)
		{
			_log.DebugFormat("Starting consumer for {0}", _transport.ToString());
			var tasks = _transport.Start(token);
			// started
			return Task.Factory
				.ContinueWhenAll(tasks, t => _log.DebugFormat("Stopped consumer for {0}", _transport.ToString()));
		}

		bool TransportOnMessageRecieved(UnpackedMessage arg)
		{
			return _dispatcher.DispatchMessage(arg);
		}
	}
}