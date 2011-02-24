#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Autofac;
using Lokad.Cqrs.Directory;
using Lokad.Cqrs.Dispatch;
using Lokad.Cqrs.Durability;
using Lokad.Cqrs.Evil;
using Lokad.Cqrs.Extensions;
using System.Linq;
using Lokad.Cqrs.Logging;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Consume
{
	public sealed class HandleMessagesModule : Module
	{
		readonly Filter<MessageMapping> _filter = new Filter<MessageMapping>();
		HashSet<string> _queueNames = new HashSet<string>();
		Func<ILifetimeScope, MessageDirectory, IMessageDispatcher> _dispatcher;


		Action<ConsumingProcess, IComponentContext> _applyToTransport = (transport, context) => { };

		public HandleMessagesModule()
		{
			SleepWhenNoMessages = BuildDecayPolicy(1.Seconds());
			ListenToQueue("azure-messages");

			WithSingleConsumer();
		}

		public HandleMessagesModule ApplyToTransport(Action<ConsumingProcess, IComponentContext> config)
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
		
		public Func<uint, TimeSpan> SleepWhenNoMessages { get; set; }

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
			var provider = context.Resolve<ILogProvider>();
			var log = provider.CreateLog<HandleMessagesModule>();

			var queueNames = _queueNames.ToArray();

			if (queueNames.Length == 0)
				throw Errors.InvalidOperation("No queue names are specified. Please use ListenTo method");

			
			var builder = context.Resolve<MessageDirectoryBuilder>();
			var filter = _filter.BuildFilter();
			var directory = builder.BuildDirectory(filter);
			log.DebugFormat("Discovered {0} messages", directory.Messages.Count);

			DebugPrintIfNeeded(log, directory);


			var dispatcher = _dispatcher(context.Resolve<ILifetimeScope>(), directory);
			var account = context.Resolve<CloudStorageAccount>();
			var serializer = context.Resolve<IMessageSerializer>();
			var queues =  queueNames.ToArray(n => new AzureReadQueue(account, n, provider, serializer));
			var transport = new ConsumingProcess(provider, dispatcher, SleepWhenNoMessages, queues);

			_applyToTransport(transport, context);
			

			return transport;
		}

		void DebugPrintIfNeeded(ILog log, MessageDirectory directory)
		{
			if (DebugPrintsMessageTree)
			{
				foreach (var info in directory.Messages)
				{
					log.DebugFormat("{0} : {1}", info.MessageType.Name, ExtendIEnumerable.JoinStrings(info.AllConsumers.Select(c => c.FullName), "; "));
				}
			}
			if (DebugPrintsConsumerTree)
			{
				foreach (var info in directory.Consumers)
				{
					log.DebugFormat("{0} : {1}", info.ConsumerType.FullName, ExtendIEnumerable.JoinStrings(info.MessageTypes.Select(c => c.Name), "; "));
				}
			}
		}

		protected override void Load(ContainerBuilder builder)
		{
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