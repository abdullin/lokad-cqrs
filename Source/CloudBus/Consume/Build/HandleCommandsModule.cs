#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Autofac;
using CloudBus.Domain;
using CloudBus.Transport;
using Lokad;

namespace CloudBus.Consume.Build
{
	public sealed class HandleCommandsModule : Module
	{
		Func<Type, bool> _commandFilter;
		HashSet<string> _queueNames = new HashSet<string>();

		public HandleCommandsModule()
		{
			IsolationLevel = IsolationLevel.RepeatableRead;
			NumberOfThreads = 1;
			SleepWhenNoMessages = AzureQueuePolicy.BuildDecayPolicy(1.Seconds());

			LogName = "Commands";
			ListenTo("azure-command");
			ConsumeMessages(t => true);
		}

		public int NumberOfThreads { get; set; }

		public IsolationLevel IsolationLevel { get; set; }
		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

		public string LogName { get; set; }

		public void ConsumeMessages(Func<Type, bool> messageFilter)
		{
			_commandFilter = messageFilter;
		}

		IBusProcess ConfigureComponent(IComponentContext context)
		{
			var log = context.Resolve<ILogProvider>().CreateLog<HandleCommandsModule>();

			var queueNames = _queueNames.ToArray();
			var transportConfig = new AzureQueueTransportConfig(
				LogName,
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