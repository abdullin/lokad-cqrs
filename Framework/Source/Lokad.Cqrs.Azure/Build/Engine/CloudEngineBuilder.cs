#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Consume.Build;
using Lokad.Cqrs.Domain;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Scheduled.Build;
using Lokad.Cqrs.Sender;
using Lokad.Cqrs.Transport;
using Lokad.Cqrs.Views;

namespace Lokad.Cqrs
{
	

	/// <summary>
	/// Fluent API for creating and configuring <see cref="ICloudEngineHost"/>
	/// </summary>
	public class CloudEngineBuilder : Syntax, ISyntax<ContainerBuilder>
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public AutofacBuilderForLogging Logging { get { return new AutofacBuilderForLogging(_builder); } }
		public AutofacBuilderForSerialization Serialization { get { return new AutofacBuilderForSerialization(_builder);} }
		public AutofacBuilderForAzure Azure { get { return new AutofacBuilderForAzure(_builder);}}

		public CloudEngineBuilder()
		{
			// System presets
			Logging.LogToTrace();
			Serialization.UseBinaryFormatter();
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterInstance(SimpleMessageProfiler.Instance);

			// Azure presets
			Azure.UseDevelopmentStorageAccount();
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<CloudSettingsProvider>().As<ISettingsProvider>().SingleInstance();


			// some defaults
			_builder.RegisterType<CloudEngineHost>().As<ICloudEngineHost>().SingleInstance();
		}
	
		/// <summary>
		/// Adds Message Handling Feature to the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageHandler(Action<HandleMessagesModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Adds Task Scheduling Feature to the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddScheduler(Action<ScheduledModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder DomainIs(Action<DomainBuildModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder AddMessageClient(Action<SenderModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Configures the view mappings for the instance of <see cref="ICloudEngineHost"/> and provides <see cref="IWriteViews"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder Views(Action<ViewBuildModule> config)
		{
			var module = new ViewBuildModule(ViewModuleRole.Writer);
			config(module);
			Target.RegisterModule(module);
			return this;
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