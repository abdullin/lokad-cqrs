#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Domain;
using Lokad.Cqrs.Transport;

namespace Lokad.Cqrs.Consume.Build
{
	

	public sealed class HandleMessagesModule : Module
	{
		readonly Filter<MessageMapping> _filter = new Filter<MessageMapping>();
		HashSet<string> _queueNames = new HashSet<string>();

		Func<ILifetimeScope, IMessageDirectory, IMessageDispatcher> _dispatcher;

		public HandleMessagesModule()
		{
			IsolationLevel = IsolationLevel.RepeatableRead;
			NumberOfThreads = 1;
			SleepWhenNoMessages = AzureQueuePolicy.BuildDecayPolicy(1.Seconds());

			LogName = "Messages";
			ListenTo("azure-messages");

			WithSingleConsumer();
		}

		public HandleMessagesModule WithSingleConsumer()
		{
			_dispatcher = (context, directory) =>
				{
					var d = new DispatchesSingleMessage(context, directory);
					d.Init();
					return d;
				};

			return this;
		}

		public HandleMessagesModule WithMultipleConsumers()
		{
			_dispatcher = (scope, directory) =>
				{
					var d = new DispatchesMultipleMessagesToSharedScope(scope, directory);
					d.Init();
					return d;
				};

			return this;
		}

		public int NumberOfThreads { get; set; }

		public IsolationLevel IsolationLevel { get; set; }
		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

		public string LogName { get; set; }
		public bool DebugPrintsMessageTree { get; set; }
		public bool DebugPrintsConsumerTree { get; set; }

		public HandleMessagesModule WhereMessages(Func<MessageMapping, bool> filter)
		{
			_filter.Where(filter);
			return this;
		}

		public HandleMessagesModule WhereMessagesInherit<TInterface>()
		{
			return WhereMessages(mm => typeof (TInterface).IsAssignableFrom(mm.Message));
		}

		public HandleMessagesModule ListenTo(params string[] queueNames)
		{
			_queueNames = queueNames.ToSet();
			return this;
		}

		IStartable ConfigureComponent(IComponentContext context)
		{
			var log = context.Resolve<ILogProvider>().CreateLog<HandleMessagesModule>();

			var queueNames = _queueNames.ToArray();
			var transportConfig = new AzureQueueTransportConfig(
				LogName,
				NumberOfThreads,
				IsolationLevel,
				queueNames,
				SleepWhenNoMessages);

			var transport = context.Resolve<IMessageTransport>(TypedParameter.From(transportConfig));

			var builder = context.Resolve<IMessageDirectoryBuilder>();
			var filter = _filter.BuildFilter();
			var directory = builder.BuildDirectory(filter);

			log.DebugFormat("Discovered {0} messages", directory.Messages.Length);

			DebugPrintIfNeeded(log, directory);

			var dispatcher = _dispatcher(context.Resolve<ILifetimeScope>(), directory);
			var consumer = context.Resolve<ConsumingProcess>(
				TypedParameter.From(transport),
				TypedParameter.From(dispatcher));

			log.DebugFormat("Use {0} threads to listen to {1}", NumberOfThreads, queueNames.Join("; "));
			return consumer;
		}

		void DebugPrintIfNeeded(ILog log, IMessageDirectory directory)
		{
			if (DebugPrintsMessageTree)
			{
				foreach (var info in directory.Messages)
				{
					log.DebugFormat("{0} : {1}", info.MessageType.Name, info.AllConsumers.Select(c => c.FullName).Join("; "));
				}
			}
			if (DebugPrintsConsumerTree)
			{
				foreach (var info in directory.Consumers)
				{
					log.DebugFormat("{0} : {1}", info.ConsumerType.FullName, info.MessageTypes.Select(c => c.Name).Join("; "));
				}
			}
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ConsumingProcess>();
			builder.Register(ConfigureComponent);
		}
	}
}