#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
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
			Builder.RegisterType<AzurePartitionFactory>().As<IPartitionInboxFactory, IEngineProcess>().SingleInstance();

			Builder
				.RegisterType<MemoryPartitionFactory>()
				.As<IPartitionInboxFactory, IQueueWriterFactory, IEngineProcess>()
				.SingleInstance();



			Builder.RegisterType<SingleThreadConsumingProcess>();
			Builder.RegisterType<MessageDuplicationManager>().SingleInstance();

			// some defaults
			Builder.RegisterType<CloudEngineHost>().SingleInstance();
		}

		/// <summary>
		/// Adds Message Handling Feature to the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageHandler(Action<HandleMessagesModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudEngineBuilder DomainIs(Action<DomainBuildModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="CloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageClient(Action<SendMessageModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Builds this <see cref="CloudEngineHost"/>.
		/// </summary>
		/// <returns>new instance of cloud engine host</returns>
		public CloudEngineHost Build()
		{
			ILifetimeScope container = Builder.Build();
			return container.Resolve<CloudEngineHost>(TypedParameter.From(container));
		}
	}
}