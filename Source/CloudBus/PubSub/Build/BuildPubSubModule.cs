using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Autofac;
using Bus2.Transport;
using Lokad;

namespace Bus2.PubSub.Build
{
	public sealed class BuildPubSubModule : Module
	{
		public int NumberOfThreads { get; set; }
		public IsolationLevel IsolationLevel { get; set; }
		HashSet<string> _queueNames;

		public TimeSpan SleepWhenNoMessages { get; set; }

		Action<ContainerBuilder> _registerManager;

		

		public BuildPubSubModule()
		{
			NumberOfThreads = 1;
			IsolationLevel = IsolationLevel.ReadCommitted;
			SleepWhenNoMessages = 1.Seconds();

			_queueNames = new HashSet<string>
				{
					"exchange-publish"
				};

			ManagerIs<InMemoryPublishSubscribeManager>();
		}

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

		IBusProcess BuildComponent(IComponentContext context)
		{
			var log = context.Resolve<ILogProvider>().CreateLog<BuildPubSubModule>();

			var queueNames = _queueNames.ToArray();
			var transportConfig = new AzureQueueTransportConfig(
				NumberOfThreads,
				IsolationLevel,
				queueNames, SleepWhenNoMessages);

			var transport = context.Resolve<IMessageTransport>(TypedParameter.From(transportConfig));
			var consumer = context.Resolve<PublishSubscribeProcess>(TypedParameter.From(transport));

			log.DebugFormat("Use {0} threads to listen to {1}", NumberOfThreads, queueNames.Join("; "));
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