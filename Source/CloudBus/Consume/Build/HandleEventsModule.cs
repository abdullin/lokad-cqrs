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
	public sealed class HandleEventsModule : Module
	{
		Func<MessageInfo, bool> _messageFilter = type => true;
		Func<ConsumerInfo, bool> _handlerFilter = type => true;

		HashSet<string> _queueNames = new HashSet<string>();

		public HandleEventsModule()
		{
			IsolationLevel = IsolationLevel.RepeatableRead;
			NumberOfThreads = 1;
			SleepWhenNoMessages = AzureQueuePolicy.BuildDecayPolicy(1.Seconds());

			LogName = "Events";
			ListenTo("azure-event");
		}

		public int NumberOfThreads { get; set; }

		public IsolationLevel IsolationLevel { get; set; }
		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

		public string LogName { get; set; }

		public void WhereMessages(Func<MessageInfo, bool> messageFilter)
		{
			_messageFilter = messageFilter;
		}
		public void WhereConsumers(Func<ConsumerInfo, bool> handlerFilter)
		{
			_handlerFilter = handlerFilter;
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
				LogName,
				NumberOfThreads,
				IsolationLevel,
				queueNames,
				SleepWhenNoMessages);

			var transport = context.Resolve<IMessageTransport>(TypedParameter.From(transportConfig));

			var directory = context
				.Resolve<IMessageDirectory>()
				.WhereConsumers(_handlerFilter)
				.WhereMessages(_messageFilter);

			log.DebugFormat("Discovered {0} events", directory.Messages.Length);

			var dispatcher = new DispatchesToManyConsumers(context.Resolve<ILifetimeScope>(), directory);
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