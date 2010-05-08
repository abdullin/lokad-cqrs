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
	public sealed class HandleCommandsModule : Module
	{
		public int NumberOfThreads { get; set; }
		HashSet<string> _queueNames = new HashSet<string>();
		public IsolationLevel IsolationLevel { get; set; }
		public TimeSpan SleepWhenNoMessages { get; set; }

		Func<Type, bool> _commandFilter;

		public HandleCommandsModule()
		{
			IsolationLevel = IsolationLevel.RepeatableRead;
			NumberOfThreads = 1;
			SleepWhenNoMessages = 1.Seconds();
			
			ListenTo("azure-command");
			ConsumeMessages(t => true);
		}


		public void ConsumeMessages(Func<Type, bool> messageFilter)
		{
			_commandFilter = messageFilter;
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

			var commands = directory
				.Messages
				.Where(info => _commandFilter(info.MessageType))
				.ToArray();

			log.DebugFormat("Discovered {0} commands", commands.Length);

			ThrowIfCommandHasMultipleConsumers(commands);
			var dispatcher = new DispatchesToSingleConsumer(context.Resolve<ILifetimeScope>(), commands, directory);
			dispatcher.Init();

			var consumer = context.Resolve<ConsumingProcess>(
				TypedParameter.From(transport),
				TypedParameter.From<IMessageDispatcher>(dispatcher));

			log.DebugFormat("Use {0} threads to listen to {1}", NumberOfThreads, queueNames.Join("; "));
			return consumer;
		}

		void ThrowIfCommandHasMultipleConsumers(MessageInfo[] commands)
		{
			var multipleConsumers = commands
				.Where(c => c.DirectConsumers.Length > 1)
				.ToArray(c => c.MessageType.FullName);

			if (multipleConsumers.Any())
			{
				throw new InvalidOperationException(
					"These messages have multiple consumers. Did you intend to declare them as events? " +
						multipleConsumers.Join(Environment.NewLine));
			}
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ConsumingProcess>();
			builder.Register(ConfigureComponent);
		}

		public void ListenTo(params string[] queueNames)
		{
			_queueNames = queueNames.ToSet();
		}
	}
}