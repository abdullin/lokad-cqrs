#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Consume.Build;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.PubSub.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Scheduled.Build;
using Lokad.Cqrs.Sender.Build;
using Lokad.Cqrs.Serialization;
using Lokad.Cqrs.Transport;
using Lokad.Diagnostics;
using Lokad.Quality;
using Lokad.Settings;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Fluent API for creating and configuring <see cref="ICloudEngineHost"/>
	/// </summary>
	public class CloudEngineBuilder : Syntax
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudEngineBuilder()
		{
			CloudStorageAccountIsDev();

			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterInstance(SimpleMessageProfiler.Instance);

			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<DefaultCloudEngineHost>().As<ICloudEngineHost>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<CloudSettingsProvider>().As<IProfileSettings, ISettingsProvider>().SingleInstance();
		}

		/// <summary>
		/// Uses default Development storage account for Windows Azure
		/// </summary>
		/// <returns>same builder for inling multiple configuration statements</returns>
		/// <remarks>This option is enabled by default</remarks>
		public CloudEngineBuilder CloudStorageAccountIsDev()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		/// <summary>
		/// Uses development storage account defined in the configuration setting.
		/// </summary>
		/// <param name="name">The name of the configuration value to look up.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder CloudStorageAccountIsFromConfig([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			_builder.Register(c =>
				{
					var value = c.Resolve<IProfileSettings>()
						.GetString(name)
						.ExposeException("Failed to load account from '{0}'", name);
					return CloudStorageAccount.Parse(value);
				}).SingleInstance();
			return this;
		}


		/// <summary>
		/// Adds Publish Subscribe Feature to the instance of <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder PublishSubscribe(Action<BuildPubSubModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		/// <summary>
		/// Adds Message Handling Feature to the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder HandleMessages(Action<HandleMessagesModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		/// <summary>
		/// Adds Task Scheduling Feature to the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder RunTasks(Action<ScheduledModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder Domain(Action<DomainBuildModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="ICloudEngineHost"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder SendMessages(Action<SenderModule> config)
		{
			ConfigureWith(config);
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

		void ConfigureWith<TModule>(Action<TModule> config)
			where TModule : IModule, new()
		{
			var module = new TModule();
			config(module);
			_builder.RegisterModule(module);
		}

		/// <summary>
		/// Registers custom module.
		/// </summary>
		/// <param name="module">The custom module to register.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudEngineBuilder RegisterModule(IModule module)
		{
			_builder.RegisterModule(module);
			return this;
		}
	}
}