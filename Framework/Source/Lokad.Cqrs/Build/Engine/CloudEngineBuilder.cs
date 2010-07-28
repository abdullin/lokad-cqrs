#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Consume.Build;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.PubSub.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Scheduled.Build;
using Lokad.Cqrs.Sender.Build;
using Lokad.Cqrs.Serialization;
using Lokad.Cqrs.Transport;
using Lokad.Diagnostics;
using Lokad.Messaging;
using Lokad.Settings;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Fluent API for creating and configuring <see cref="ICloudEngineHost"/>
	/// </summary>
	public class CloudEngineBuilder :
		ISyntax<ContainerBuilder>,
		ISyntax<ContainerBuilder, IRealtimeNotifier>
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudEngineBuilder()
		{
			this.CloudStorageAccountIsDev();

			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullRealtimeNotifier.Instance);
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterInstance(SimpleMessageProfiler.Instance);

			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<DefaultCloudEngineHost>().As<ICloudEngineHost>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<CloudSettingsProvider>().As<IProfileSettings, ISettingsProvider>().SingleInstance();
		}

		/// <summary>
		/// Adds Publish Subscribe Feature to the instance of <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder PublishSubscribe(Action<BuildPubSubModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Adds Message Handling Feature to the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder HandleMessages(Action<HandleMessagesModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Adds Task Scheduling Feature to the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder RunTasks(Action<ScheduledModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder Domain(Action<DomainBuildModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder SendMessages(Action<SenderModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Builds this <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <returns>new instance of cloud engine host</returns>
		public ICloudEngineHost Build()
		{
			var container = _builder.Build();
			return container.Resolve<ICloudEngineHost>(TypedParameter.From(container));
		}

		public ContainerBuilder Target
		{
			get { return _builder; }
		}
	}
}