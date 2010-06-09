#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using CloudBus.Domain.Build;
using CloudBus.Queue;
using CloudBus.Serialization;
using CloudBus.Transport;
using Lokad.Diagnostics;
using Lokad.Settings;
using Microsoft.WindowsAzure;

namespace CloudBus.Build.Client
{
	public sealed class ClientBusBuilder
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public ClientBusBuilder()
		{
			CloudStorageAccountIsDev();

			_builder.RegisterType<CloudSettingsProvider>().As<IProvideBusSettings, ISettingsProvider>().SingleInstance();
			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullBusProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<ClientBus>().SingleInstance();
		}

		public ClientBusBuilder CloudStorageAccountIsDev()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		public ClientBusBuilder CloudStorageAccountIsFromConfig(string name)
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

		public ClientBusBuilder Domain(Action<DomainBuildModule> configuration)
		{
			ConfigureWith(configuration);
			return this;
		}

		void ConfigureWith<TModule>(Action<TModule> config)
			where TModule : IModule, new()
		{
			var module = new TModule();
			config(module);
			_builder.RegisterModule(module);
		}


		public ClientBus BuildFor(string defaultQueue)
		{
			var container = _builder.Build();
			return container.Resolve<ClientBus>(
				TypedParameter.From(defaultQueue));
		}

		public ClientBusBuilder RegisterModule(IModule module)
		{
			_builder.RegisterModule(module);
			return this;
		}
	}
}