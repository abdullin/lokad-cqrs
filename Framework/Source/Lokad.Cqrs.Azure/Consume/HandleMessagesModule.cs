#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Transactions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Directory;
using Lokad.Cqrs.Domain;

using Lokad.Cqrs.Transport;
using System.Linq;

namespace Lokad.Cqrs.Consume.Build
{
	public sealed class HandleMessagesModule : Module
	{
		readonly Filter<MessageMapping> _filter = new Filter<MessageMapping>();
		HashSet<string> _queueNames = new HashSet<string>();
		Func<ILifetimeScope, IMessageDirectory, IMessageDispatcher> _dispatcher;


		Action<IMessageTransport, IComponentContext> _applyToTransport = (transport, context) => { };

		public HandleMessagesModule()
		{
			IsolationLevel = IsolationLevel.RepeatableRead;
			NumberOfThreads = 1;
			SleepWhenNoMessages = AzureQueuePolicy.BuildDecayPolicy(1.Seconds());

			LogName = "Messages";
			ListenToQueue("azure-messages");

			WithSingleConsumer();
		}

		public HandleMessagesModule ApplyToTransport(Action<IMessageTransport, IComponentContext> config)
		{
			_applyToTransport += config;
			return this;
		}

		
		public HandleMessagesModule WhenMessageHandlerFails(Action<UnpackedMessage, Exception> handler)
		{
			return ApplyToTransport((transport, context) =>
				{
					transport.MessageHandlerFailed += handler;
					context.WhenDisposed(() => transport.MessageHandlerFailed -= handler);
				});
		}

		
		public HandleMessagesModule WhenMessageArrives(Func<UnpackedMessage, bool> interceptor)
		{
			return ApplyToTransport((transport, context) =>
				{
					transport.MessageReceived += interceptor;
					context.WhenDisposed(() => transport.MessageReceived -= interceptor);
				});
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

		/// <summary>
		/// Gets or sets the number of threads.
		/// </summary>
		/// <value>The number of threads.</value>
		public int NumberOfThreads { get; set; }

		/// <summary>
		/// Gets or sets the isolation level.
		/// </summary>
		/// <value>The isolation level.</value>
		public IsolationLevel IsolationLevel { get; set; }
		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

		public string LogName { get; set; }
		public bool DebugPrintsMessageTree { get; set; }
		public bool DebugPrintsConsumerTree { get; set; }

		/// <summary>
		/// Adds custom filters for <see cref="MessageMapping"/>, that will be used
		/// for configuring this message handler.
		/// </summary>
		/// <param name="filter">The filter.</param>
		/// <returns></returns>
		public HandleMessagesModule WhereMappings(Func<MessageMapping, bool> filter)
		{
			_filter.Where(filter);
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
			var log = context.Resolve<ILogProvider>().CreateLog<HandleMessagesModule>();

			var queueNames = _queueNames.ToArray();

			if (queueNames.Length == 0)
				throw Errors.InvalidOperation("No queue names are specified. Please use ListenTo method");

			var transportConfig = new AzureQueueTransportConfig(
				LogName,
				NumberOfThreads,
				IsolationLevel,
				queueNames,
				SleepWhenNoMessages);

			var transport = context.Resolve<IMessageTransport>(TypedParameter.From(transportConfig));

			_applyToTransport(transport, context);


			var builder = context.Resolve<IMessageDirectoryBuilder>();
			var filter = _filter.BuildFilter();
			var directory = builder.BuildDirectory(filter);

			log.DebugFormat("Discovered {0} messages", directory.Messages.Count);

			DebugPrintIfNeeded(log, directory);

			var dispatcher = _dispatcher(context.Resolve<ILifetimeScope>(), directory);
			var consumer = context.Resolve<ConsumingProcess>(
				TypedParameter.From(transport),
				TypedParameter.From(dispatcher));



			log.DebugFormat("Use {0} threads to listen to {1}", NumberOfThreads, ExtendIEnumerable.Join(queueNames, "; "));
			return consumer;
		}

		void DebugPrintIfNeeded(ILog log, IMessageDirectory directory)
		{
			if (DebugPrintsMessageTree)
			{
				foreach (var info in directory.Messages)
				{
					log.DebugFormat("{0} : {1}", info.MessageType.Name, ExtendIEnumerable.Join(info.AllConsumers.Select(c => c.FullName), "; "));
				}
			}
			if (DebugPrintsConsumerTree)
			{
				foreach (var info in directory.Consumers)
				{
					log.DebugFormat("{0} : {1}", info.ConsumerType.FullName, ExtendIEnumerable.Join(info.MessageTypes.Select(c => c.Name), "; "));
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