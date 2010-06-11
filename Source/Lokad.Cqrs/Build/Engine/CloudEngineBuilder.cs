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
using Lokad.Settings;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs
{
	public class CloudEngineBuilder
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

		public CloudEngineBuilder CloudStorageAccountIsDev()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		public CloudEngineBuilder CloudStorageAccountIsFromConfig(string name)
		{
			_builder.Register(c =>
				{
					var value = c.Resolve<IProfileSettings>()
						.GetString(name)
						.ExposeException("Failed to load account from '{0}'", name);
					return CloudStorageAccount.Parse(value);
				}).SingleInstance();
			return this;
		}


		public CloudEngineBuilder PublishSubscribe(Action<BuildPubSubModule> action)
		{
			ConfigureWith(action);
			return this;
		}

		public CloudEngineBuilder HandleMessages(Action<HandleMessagesModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudEngineBuilder RunTasks(Action<ScheduledModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudEngineBuilder Domain(Action<DomainBuildModule> configuration)
		{
			ConfigureWith(configuration);
			return this;
		}

		public CloudEngineBuilder SendMessages(Action<SenderModule> configuration)
		{
			ConfigureWith(configuration);
			return this;
		}

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

		public CloudEngineBuilder RegisterModule(IModule module)
		{
			_builder.RegisterModule(module);
			return this;
		}
	}
}