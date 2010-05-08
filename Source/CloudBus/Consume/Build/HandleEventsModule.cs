using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Autofac;
using Bus2.Domain;
using Bus2.Transport;
using Lokad;

namespace Bus2.Consume.Build
{
	public sealed class HandleEventsModule : Module
	{
		public int NumberOfThreads { get; set; }
		HashSet<string> _queueNames = new HashSet<string>();
		public IsolationLevel IsolationLevel { get; set; }
		public TimeSpan SleepWhenNoMessages { get; set; }

		Func<Type, bool> _eventFilter;

		public HandleEventsModule()
		{
			IsolationLevel = IsolationLevel.RepeatableRead;
			NumberOfThreads = 1;
			SleepWhenNoMessages = 1.Seconds();

			ListenTo("azure-event");
			ConsumeMessages(t => true);
		}

		public void ConsumeMessages(Func<Type, bool> messageFilter)
		{
			_eventFilter = messageFilter;
		}

		public void ListenTo(params string[] queueNames)
		{
			_queueNames = queueNames.ToSet();
		}

		IBusProcess ConfigureComponent(IComponentContext context)
		{
			var log = context.Resolve<ILogProvider>().CreateLog<HandleCommandsModule>();

			var queueNames = _queueNames.ToArray();
			var transportConfig = new AzureQueueTransportConfig(
				NumberOfThreads,
				IsolationLevel,
				queueNames,
				SleepWhenNoMessages);

			var transport = context.Resolve<IMessageTransport>(TypedParameter.From(transportConfig));

			var directory = context.Resolve<IMessageDirectory>();

			var events = directory
				.Messages
				.Where(info => _eventFilter(info.MessageType))
				.ToArray();

			log.DebugFormat("Discovered {0} events", events.Length);
			
			var dispatcher = new DispatchesToManyConsumers(context.Resolve<ILifetimeScope>(), events, directory);
			dispatcher.Init();

			var consumer = context.Resolve<ConsumingProcess>(
				TypedParameter.From(transport),
				TypedParameter.From<IMessageDispatcher>(dispatcher));

			log.DebugFormat("Use {0} threads to listen to {1}", NumberOfThreads, queueNames.Join("; "));
			return consumer;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ConsumingProcess>();
			builder.Register(ConfigureComponent);
		}
	}
}