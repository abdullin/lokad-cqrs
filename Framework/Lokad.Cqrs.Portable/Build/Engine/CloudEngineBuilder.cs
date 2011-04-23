#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.Logging;
using Lokad.Cqrs.Feature.MemoryPartition;

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lokad.Cqrs.Build.Engine
{
	/// <summary>
	/// Fluent API for creating and configuring <see cref="CloudEngineHost"/>
	/// </summary>
	public class CloudEngineBuilder : BuildSyntaxHelper
	{
		HashSet<IModule> _moduleEnlistments = new HashSet<IModule>();

		bool IsEnlisted<TModule>() where TModule : IModule
		{
			return _moduleEnlistments.Count(x => x is TModule) > 0;
		}

		public void Enlist<TModule>(Action<TModule> config) where TModule : IModule, new()
		{
			var m = new TModule();
			config(m);
			_moduleEnlistments.Add(m);
		}

		public void Enlist(IModule module)
		{
			_moduleEnlistments.Add(module);
		}


		public CloudEngineBuilder AddMemoryPartition(string[] queues, Action<MemoryPartitionModule> config)
		{
			foreach (var queue in queues)
			{
				Assert(!ContainsQueuePrefix(queue), "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
			}
			var module = new MemoryPartitionModule(queues);
			config(module);
			Enlist(module);
			return this;
		}

		public CloudEngineBuilder RegisterSystemObserver(ISystemObserver observer)
		{
			Builder.RegisterInstance(observer);
			return this;
		}


		public CloudEngineBuilder AddMemoryPartition(params string[] queues)
		{
			return AddMemoryPartition(queues, m => { });
		}

		public CloudEngineBuilder RegisterInstance<T>(T instance) where T : class
		{
			Builder.RegisterInstance(instance);
			return this;
		}

		public CloudEngineBuilder AddMemoryPartition(string queueName, Action<MemoryPartitionModule> config)
		{
			return AddMemoryPartition(new string[] {queueName}, config);
		}

		public CloudEngineBuilder AddMemoryRouter(string queueName, Func<MessageEnvelope, string> config)
		{
			return AddMemoryPartition(queueName, m => m.Dispatch<DispatchMessagesToRoute>(x => x.SpecifyRouter(config)));
		}


		/// <summary>
		/// Configures the message domain for the instance of <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudEngineBuilder DomainIs(Action<DomainBuildModule> config)
		{
			Enlist(config);
			return this;
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageClient(string queueName)
		{
			var m = new SendMessageModule(queueName);
			Enlist(m);
			return this;
		}

		public readonly ContainerBuilder Builder = new ContainerBuilder();

		public CloudEngineBuilder Serialization(Action<AutofacBuilderForSerialization> config)
		{
			var m = new AutofacBuilderForSerialization();
			config(m);
			Enlist(m);
			return this;
		}

		/// <summary>
		/// Builds this <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <returns>new instance of cloud engine host</returns>
		public CloudEngineHost Build()
		{
			// nonconditional registrations
			// System presets
			RegisterSystemObserver(new TraceSystemObserver());

			Builder.RegisterType<DispatcherProcess>();
			Builder.RegisterType<MessageDuplicationManager>().SingleInstance();

			// some defaults
			Builder.RegisterType<CloudEngineHost>().SingleInstance();

			// conditional registrations and defaults
			if (!IsEnlisted<DomainBuildModule>())
			{
				DomainIs(m =>
					{
						m.WithDefaultInterfaces();
						m.InUserAssemblies();
					});
			}
			if (!IsEnlisted<AutofacBuilderForSerialization>())
			{
				Serialization(x => x.UseDataContractSerializer());
			}

			if (IsEnlisted<MemoryPartitionModule>())
			{
				Builder.RegisterType<MemoryPartitionFactory>().As<IQueueWriterFactory, IEngineProcess, MemoryPartitionFactory>().
					SingleInstance();
			}


			foreach (var module in _moduleEnlistments)
			{
				Builder.RegisterModule(module);
			}

			var container = Builder.Build();
			var host = container.Resolve<CloudEngineHost>(TypedParameter.From(container));
			host.Initialize();
			return host;
		}
	}
}