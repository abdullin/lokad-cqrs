#region (c) 2010-2011 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Threading;
using System.Threading.Tasks;
using Lokad.Cqrs.Transport;


namespace Lokad.Cqrs.Consume
{
	
	public sealed class ConsumingProcess : IEngineProcess
	{
		readonly IMessageDispatcher _dispatcher;
		readonly ILog _log;
		readonly AzureQueueTransport _transport;

		public ConsumingProcess(AzureQueueTransport transport, ILogProvider provider, IMessageDispatcher dispatcher)
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

		void TransportOnMessageRecieved(UnpackedMessage arg)
		{
			_dispatcher.DispatchMessage(arg);
		}
	}
}