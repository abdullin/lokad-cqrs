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
	

	public sealed class HandleMessagesModule : Module
	{
		readonly HashSet<Func<DomainMessageMapping, bool>> _filters = new HashSet<Func<DomainMessageMapping, bool>>();
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
					var d = new DispatchesToSingleConsumer(context, directory);
					d.Init();
					return d;
				};

			return this;
		}

		public HandleMessagesModule WithMultipleConsumers()
		{
			_dispatcher = (scope, directory) =>
				{
					var d = new DispatchesToManyConsumers(scope, directory);
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

		public HandleMessagesModule Where(Func<DomainMessageMapping, bool> filter)
		{
			_filters.Add(filter);
			return this;
		}

		public HandleMessagesModule ListenTo(params string[] queueNames)
		{
			_queueNames = queueNames.ToSet();
			return this;
		}

		IBusProcess ConfigureComponent(IComponentContext context)
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

			var scan = context.Resolve<IMessageScan>();
			var directory = scan.BuildDirectory(_filters);

			log.DebugFormat("Discovered {0} messages", directory.Messages.Length);

			if (DebugPrintsMessageTree)
			{
				foreach (var messageInfo in directory.Messages)
				{
					log.DebugFormat("{0} : {1}", messageInfo.MessageType.Name, messageInfo.Implements.Select(m => m.MessageType.Name).Join(", "));
					log.DebugFormat("Consumed by {0}", messageInfo.AllConsumers.Select(c => c.Name).Join(", "));
				}
			}

			var dispatcher = _dispatcher(context.Resolve<ILifetimeScope>(), directory);
			var consumer = context.Resolve<ConsumingProcess>(
				TypedParameter.From(transport),
				TypedParameter.From(dispatcher));

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