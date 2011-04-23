#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Feature.Logging;
using Lokad.Cqrs.Feature.MemoryPartition;

namespace Lokad.Cqrs.Build.Client
{
	/// <summary>
	/// Fluent API for creating and configuring <see cref="CloudClient"/>
	/// </summary>
// ReSharper disable UnusedMember.Global
	public sealed class CloudClientBuilder : BuildSyntaxHelper
	{
		public readonly ContainerBuilder Builder = new ContainerBuilder();

		public CloudClientBuilder()
		{
			// default serialization


			Serialization(x => x.AutoDetectSerializer());

			Builder.RegisterInstance(new TraceSystemObserver());

			Builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
			Builder.RegisterType<MemoryPartitionFactory>().As<IQueueWriterFactory>().SingleInstance();
			

			Builder.RegisterType<CloudClient>().SingleInstance();
		}

	

		/// <summary>
		/// Creates default message sender for the instance of <see cref="CloudClient"/>
		/// </summary>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudClientBuilder AddMessageClient(string queueName)
		{
			Builder.RegisterModule(new SendMessageModule(queueName));
			return this;
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="CloudClient"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudClientBuilder Domain(Action<DomainBuildModule> config)
		{
			var m = new DomainBuildModule();
			config(m);
			Builder.RegisterModule(m);
			return this;
		}

		public CloudClientBuilder Azure(Action<ModuleForAzure> config)
		{
			var m = new ModuleForAzure();
			config(m);
			Builder.RegisterModule(m);
			return this;
		}

		public CloudClientBuilder Serialization(Action<AutofacBuilderForSerialization> config)
		{
			var m = new AutofacBuilderForSerialization();
			config(m);
			Builder.RegisterModule(m);
			return this;
		}
		
		public CloudClient Build()
		{
			return Builder.Build().Resolve<CloudClient>();
		}
	}
}