using System;
using Autofac;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Feature.MemoryPartition;

namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{
	public sealed class MemoryPartitionModule : Module
	{
		readonly string[] _memoryQueues;
		Tuple<Type, Action<ISingleThreadMessageDispatcher>> _dispatcher;
		readonly MessageDirectoryFilter _filter = new MessageDirectoryFilter();

		public MemoryPartitionModule WhereFilter(Action<MessageDirectoryFilter> filter)
		{
			filter(_filter);
			return this;
		}
		

		public MemoryPartitionModule(string[] memoryQueues)
		{
			_memoryQueues = memoryQueues;
			Dispatch<DispatchEventToMultipleConsumers>(x => { });
		}

		public MemoryPartitionModule Dispatch<TDispatcher>(Action<TDispatcher> configure)
			where TDispatcher : class,ISingleThreadMessageDispatcher
		{
			_dispatcher = Tuple.Create(typeof(TDispatcher), new Action<ISingleThreadMessageDispatcher>(d => configure((TDispatcher)d)));
			return this;
		}


		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType(_dispatcher.Item1);
			builder.Register(BuildConsumingProcess);
		}

		IEngineProcess BuildConsumingProcess(IComponentContext context)
		{
			var log = context.Resolve<ISystemObserver>();


			var dir = context.Resolve<MessageDirectoryBuilder>();
			var directory = dir.BuildDirectory(_filter.DoesPassFilter);
			var typedParameter = TypedParameter.From(directory);
			var dispatcher = (ISingleThreadMessageDispatcher)context.Resolve(_dispatcher.Item1, typedParameter);
			_dispatcher.Item2(dispatcher);
			dispatcher.Init();

			var factory = context.Resolve<MemoryPartitionFactory>();
			var notifier = factory.GetMemoryInbox(_memoryQueues);

			var transport = new DispatcherProcess(log, dispatcher, notifier);
			return transport;
		}
	}
}