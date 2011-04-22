#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Partition;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Feature.TestPartition;

// ReSharper disable UnusedMethodReturnValue.Global
namespace Lokad.Cqrs.Build.Engine
{
	

	/// <summary>
	/// Fluent API for creating and configuring <see cref="CloudEngineHost"/>
	/// </summary>
	public class CloudEngineBuilder : AutofacBuilderBase
	{

		public AutofacBuilderForLogging Logging { get { return new AutofacBuilderForLogging(Builder); } }
		public AutofacBuilderForSerialization Serialization { get { return new AutofacBuilderForSerialization(Builder);} }
		public AutofacBuilderForAzure Azure { get { return new AutofacBuilderForAzure(Builder);}}

		public CloudEngineBuilder()
		{
			// System presets
			Logging.LogToTrace();
			Serialization.AutoDetectSerializer();

			// Azure presets
			Azure.UseDevelopmentStorageAccount();


			// register azure partition factory
			Builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
			Builder.RegisterType<AzurePartitionFactory>().As<AzurePartitionFactory, IEngineProcess>().SingleInstance();

			Builder.RegisterType<MemoryPartitionFactory>().As<IQueueWriterFactory, IEngineProcess, MemoryPartitionFactory>().SingleInstance();



			Builder.RegisterType<SingleThreadConsumingProcess>();
			Builder.RegisterType<MessageDuplicationManager>().SingleInstance();

			// some defaults
			Builder.RegisterType<CloudEngineHost>().SingleInstance();
		}



		public CloudEngineBuilder AddMemoryPartition(string[] queues, Action<MemoryPartitionModule> config)
		{

			foreach (var queue in queues)
			{
				Buildy.Assert(!Cqrs.Build.Buildy.ContainsQueuePrefix(queue), "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
			}
			var module = new MemoryPartitionModule(queues);

			config(module);
			Builder.RegisterModule(module);
			return this;
		}


		public CloudEngineBuilder AddAzurePartition(string[] queues, Action<AzurePartitionModule> config)
		{
			foreach (var queue in queues)
			{
				Buildy.Assert(!Cqrs.Build.Buildy.ContainsQueuePrefix(queue), "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
			}
			var module = new AzurePartitionModule(queues);

			config(module);
			Builder.RegisterModule(module);
			return this;
		}

		public CloudEngineBuilder AddAzurePartition(params string[] queues)
		{
			return AddAzurePartition(queues, m => { });
		}

		public CloudEngineBuilder AddMemoryPartition(params string[] queues)
		{
			return AddMemoryPartition(queues, m => { });
		}

		public CloudEngineBuilder AddMemoryPartition(string queueName, Action<MemoryPartitionModule> config)
		{
			return AddMemoryPartition(new string[]{queueName}, config);
		}

		public CloudEngineBuilder AddMemoryRouter(string queueName, Func<MessageEnvelope, string> config)
		{
			return AddMemoryPartition(queueName, m => m.Dispatch<DispatchMessagesToRoute>(x => x.SpecifyRouter(config)));
		}



		int _domainRegistrations = 0;

		/// <summary>
		/// Configures the message domain for the instance of <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudEngineBuilder DomainIs(Action<DomainBuildModule> config)
		{
			_domainRegistrations += 1;
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageClient(string queueName)
		{
			var m = new SendMessageModule(queueName);

			Builder.RegisterModule(m);
			return this;
		}

		/// <summary>
		/// Builds this <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <returns>new instance of cloud engine host</returns>
		public CloudEngineHost Build()
		{
			if (_domainRegistrations == 0)
			{
				DomainIs(m =>
					{
						m.WithDefaultInterfaces();
						m.InUserAssemblies();
					});
			}


			ILifetimeScope container = Builder.Build();
			var host = container.Resolve<CloudEngineHost>(TypedParameter.From(container));
			host.Initialize();
			return host;
		}
	}
}