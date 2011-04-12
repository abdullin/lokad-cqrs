#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using System.Linq;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Durability;
using Microsoft.WindowsAzure;

// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs.Feature.Consume
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
			SleepWhenNoMessages = BuildDecayPolicy(1.Seconds());
			ListenToQueue("azure-messages");

			ModuleName = "Handler-" + GetHashCode().ToString("X8");

			_queueFactory = (context, s) =>
				{
					return context.Resolve<AzureReadQueue>(TypedParameter.From(s));
				};

			Dispatch<DispatchCommandBatchToSingleConsumer>();
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


		public HandleMessagesModule Dispatch<TDispatcher>(Action<TDispatcher> configure)
			where TDispatcher : class,ISingleThreadMessageDispatcher
		{
			_dispatcher = Tuple.Create(typeof (TDispatcher), new Action<ISingleThreadMessageDispatcher>(d => configure((TDispatcher) d)));
			return this;
		}

		public HandleMessagesModule Dispatch<TDispatcher>()
			where TDispatcher : class, ISingleThreadMessageDispatcher
		{
			return Dispatch<TDispatcher>(dispatcher => { });
		}
		
		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

		
		/// <summary>
		/// Adds custom filters for <see cref="MessageMapping"/>, that will be used
		/// for configuring this message handler.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public HandleMessagesModule WhereMappings(Func<MessageMapping, bool> filter)
		{
			_filter.AddFilter(filter);
			return this;
		}

		/// <summary>
		/// Adds filter to exclude all message mappings, where messages derive from the specified class
		/// </summary>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public HandleMessagesModule WhereMessagesAreNot<TMessage>()
		{
			return WhereMappings(mm => !typeof(TMessage).IsAssignableFrom(mm.Message));
		}

		/// <summary>
		/// Adds filter to include only message mappings, where messages derive from the specified class
		/// </summary>
		/// <typeparam name="TMessage">The type of the message.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public HandleMessagesModule WhereMessagesAre<TMessage>()
		{
			return WhereMappings(mm => typeof(TMessage).IsAssignableFrom(mm.Message));
		}

		/// <summary>
		/// Adds filter to include only message mappings, where consumers derive from the specified class
		/// </summary>
		/// <typeparam name="TConsumer">The type of the consumer.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public HandleMessagesModule WhereConsumersAre<TConsumer>()
		{
			return WhereMappings(mm => typeof(TConsumer).IsAssignableFrom(mm.Consumer));
		}
		/// <summary>
		/// Adds filter to exclude all message mappings, where consumers derive from the specified class
		/// </summary>
		/// <typeparam name="TConsumer">The type of the consumer.</typeparam>
		/// <returns>same module instance for chaining fluent configurations</returns>
		public HandleMessagesModule WhereConsumersAreNot<TConsumer>()
		{
			return WhereMappings(mm => !typeof(TConsumer).IsAssignableFrom(mm.Consumer));
		}

		/// <summary>
		/// Additional configuration to log the exceptions to BLOB.
		/// </summary>
		/// <param name="config">The config.</param>
		/// <returns>same module for inlining</returns>
		public HandleMessagesModule LogExceptionsToBlob(Action<ConfigureBlobSavingOnException> config)
		{
			var configurer = new ConfigureBlobSavingOnException();
			config(configurer);
			ApplyToTransport(configurer.Apply);
			return this;
		}
		/// <summary>
		/// Additional configuration to log the exceptions to BLOB.
		/// </summary>
		/// <param name="containerName">Name of the container.</param>
		/// <param name="delegates">The delegates.</param>
		/// <returns>same module for inlining</returns>
		public HandleMessagesModule LogExceptionsToBlob(string containerName, params PrintMessageErrorDelegate[] delegates)
		{
			return LogExceptionsToBlob(x =>
				{
					x.ContainerName = containerName;
					foreach (var append in delegates)
					{
						x.WithTextAppender(append);
					}
				});
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


			
			var queues =  queueNames.Select(n => _queueFactory(context,n)).ToArray();
			var transport = new SingleThreadConsumingProcess(log, dispatcher, SleepWhenNoMessages, queues);

			_applyToTransport(transport, context);
			

			return transport;
		}

		Func<IComponentContext, string, IReadQueue> _queueFactory;

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<AzureReadQueue>();

			// make sure the dispatcher is registered
			builder.RegisterType(_dispatcher.Item1);

			builder.Register(ConfigureComponent);
		}

		public static Func<uint, TimeSpan> BuildDecayPolicy(TimeSpan maxDecay)
		{
			//var seconds = (Rand.Next(0, 1000) / 10000d).Seconds();
			var seconds = maxDecay.TotalSeconds;
			return l =>
				{
					if (l >= 31)
					{
						return maxDecay;
					}

					var foo = Math.Pow(2, (l - 1)/5.0)/64d*seconds;

					return TimeSpan.FromSeconds(foo);
				};
		}
	}
}