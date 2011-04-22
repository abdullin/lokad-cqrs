#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Feature.AzurePartition.Events;

// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs.Feature.AzurePartition.Inbox
{

	public sealed class HandleMessagesModule : Module
	{
		readonly Filter<MessageMapping> _filter = new Filter<MessageMapping>();
		HashSet<string> _queueNames = new HashSet<string>();

		Tuple<Type, Action<ISingleThreadMessageDispatcher>> _dispatcher;

		public string ModuleName { get; private set; }

		Action<SingleThreadConsumingProcess, IComponentContext> _applyToTransport = (transport, context) => { };

		public HandleMessagesModule()
		{
			ListenToQueue("azure-messages");

			ModuleName = "Handler-" + GetHashCode().ToString("X8");


			
		}

		public HandleMessagesModule ApplyToTransport(Action<SingleThreadConsumingProcess, IComponentContext> config)
		{
			_applyToTransport += config;
			return this;
		}

		
		public HandleMessagesModule WhenMessageHandlerFails(Action<MessageEnvelope, Exception> handler)
		{
			throw new NotImplementedException();
			//return ApplyToTransport((transport, context) =>
			//    {
			//        transport.MessageHandlerFailed += handler;
			//        context.WhenDisposed(() => transport.MessageHandlerFailed -= handler);
			//    });
		}

		
		public HandleMessagesModule WhenMessageArrives(Action<MessageEnvelope> interceptor)
		{
			throw new NotImplementedException();
		}


		

		
		
		
	

		/// <summary>
		/// Specifies names of the queues to listen to
		/// </summary>
		/// <param name="queueNames">The queue names to listen to.</param>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public HandleMessagesModule ListenToQueue(params string[] queueNames)
		{
			_queueNames = queueNames.ToSet();
			return this;
		}

		IEngineProcess ConfigureComponent(IComponentContext context)
		{
			var log = context.Resolve<ISystemObserver>();

			var queueNames = _queueNames.ToArray();

			if (queueNames.Length == 0)
				throw Errors.InvalidOperation("No queue names are specified. Please use ListenTo method");

			
			var builder = context.Resolve<MessageDirectoryBuilder>();
			var filter = _filter.BuildFilter();
			var directory = builder.BuildDirectory(filter);

			log.Notify(new MessageDirectoryCreated(ModuleName, directory.Messages.ToArray(), directory.Consumers.ToArray()));


			var dispatcher = (ISingleThreadMessageDispatcher)context.Resolve(_dispatcher.Item1, TypedParameter.From(directory));
			_dispatcher.Item2(dispatcher);
			dispatcher.Init();

			var factory = context.Resolve<AzurePartitionFactory>();
			var notifier = factory.GetNotifier(queueNames);
			
			var transport = new SingleThreadConsumingProcess(log, dispatcher, notifier);

			_applyToTransport(transport, context);
			

			return transport;
		}

		

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<StatelessAzureQueueReader>();

			// make sure the dispatcher is registered
			builder.RegisterType(_dispatcher.Item1);

			builder.Register(ConfigureComponent);
		}
	}
}