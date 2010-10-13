#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using System.Transactions;
using Autofac;
using Lokad.Cqrs.Transport;

namespace Lokad.Cqrs.PubSub.Build
{
	public sealed class BuildPubSubModule : Module
	{
		HashSet<string> _queueNames;
		Action<ContainerBuilder> _registerManager;

		public BuildPubSubModule()
		{
			NumberOfThreads = 1;
			IsolationLevel = IsolationLevel.ReadCommitted;
			SleepWhenNoMessages = AzureQueuePolicy.BuildDecayPolicy(1.Seconds());

			LogName = "PubSub";
			_queueNames = new HashSet<string>
				{
					"exchange-publish"
				};

			ManagerIs<InMemoryPublishSubscribeManager>();
		}

		public int NumberOfThreads { get; set; }
		public IsolationLevel IsolationLevel { get; set; }

		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

		public string LogName { get; set; }

		public BuildPubSubModule ManagerIs<TManager>()
			where TManager : IPublishSubscribeManager
		{
			_registerManager = builder => builder
				.RegisterType<TManager>()
				.As<IPublishSubscribeManager>()
				.SingleInstance();

			return this;
		}

		public SubscribeSyntax ManagerIsInMemory()
		{
			var manager = new InMemoryPublishSubscribeManager();
			_registerManager = builder => builder
				.RegisterInstance(manager)
				.As<IPublishSubscribeManager>();
			return new SubscribeSyntax(manager);
		}

		public BuildPubSubModule ListenTo(params string[] names)
		{
			_queueNames = names.ToSet();
			return this;
		}

		public BuildPubSubModule WithThreads(int numberOfThreads)
		{
			NumberOfThreads = numberOfThreads;
			return this;
		}

		public void Default()
		{
		}

		IEngineProcess BuildComponent(IComponentContext context)
		{
			var log = context.Resolve<ILogProvider>().CreateLog<BuildPubSubModule>();

			var queueNames = _queueNames.ToArray();
			var transportConfig = new AzureQueueTransportConfig(
				LogName,
				NumberOfThreads,
				IsolationLevel,
				queueNames, SleepWhenNoMessages);

			var transport = context.Resolve<IMessageTransport>(TypedParameter.From(transportConfig));
			var consumer = context.Resolve<PublishSubscribeProcess>(TypedParameter.From(transport));

			log.DebugFormat("Use {0} threads to listen to {1}", NumberOfThreads, ExtendIEnumerable.Join(queueNames, "; "));
			return consumer;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<PublishSubscribeProcess>();

			_registerManager(builder);

			builder.Register(BuildComponent);
		}
	}
}