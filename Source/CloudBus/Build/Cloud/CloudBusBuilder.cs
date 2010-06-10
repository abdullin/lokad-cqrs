#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using CloudBus.Consume.Build;
using CloudBus.Domain.Build;
using CloudBus.PubSub.Build;
using CloudBus.Queue;
using CloudBus.Scheduled;
using CloudBus.Scheduled.Build;
using CloudBus.Sender;
using CloudBus.Sender.Build;
using CloudBus.Serialization;
using CloudBus.Transport;
using Lokad.Diagnostics;
using Lokad.Settings;
using Microsoft.WindowsAzure;

namespace CloudBus.Build.Cloud
{
	public class CloudBusBuilder
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudBusBuilder()
		{
			CloudStorageAccountIsDev();

			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullBusProfiler.Instance);
			_builder.RegisterInstance(SimpleMessageProfiler.Instance);

			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<DefaultCloudBusHost>().As<ICloudBusHost>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<CloudSettingsProvider>().As<IProvideBusSettings, ISettingsProvider>().SingleInstance();
		}

		public CloudBusBuilder CloudStorageAccountIsDev()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		public CloudBusBuilder CloudStorageAccountIsFromConfig(string name)
		{
			_builder.Register(c =>
				{
					var value = c.Resolve<IProvideBusSettings>()
						.GetString(name)
						.ExposeException("Failed to load account from '{0}'", name);
					return CloudStorageAccount.Parse(value);
				}).SingleInstance();
			return this;
		}


		public CloudBusBuilder PublishSubscribe(Action<BuildPubSubModule> action)
		{
			ConfigureWith(action);
			return this;
		}

		public CloudBusBuilder HandleMessages(Action<HandleMessagesModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudBusBuilder RunTasks(Action<ScheduledModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudBusBuilder Domain(Action<DomainBuildModule> configuration)
		{
			ConfigureWith(configuration);
			return this;
		}

		public CloudBusBuilder SendMessages(Action<SenderModule> configuration)
		{
			ConfigureWith(configuration);
			return this;
		}

		public ICloudBusHost Build()
		{
			var container = _builder.Build();
			return container.Resolve<ICloudBusHost>(TypedParameter.From(container));
		}

		void ConfigureWith<TModule>(Action<TModule> config)
			where TModule : IModule, new()
		{
			var module = new TModule();
			config(module);
			_builder.RegisterModule(module);
		}

		public CloudBusBuilder RegisterModule(IModule module)
		{
			_builder.RegisterModule(module);
			return this;
		}
	}
}