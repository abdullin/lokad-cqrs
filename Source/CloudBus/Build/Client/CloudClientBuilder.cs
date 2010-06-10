#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Serialization;
using Lokad.Cqrs.Transport;
using Lokad.Diagnostics;
using Lokad.Settings;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs
{
	public sealed class CloudClientBuilder
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudClientBuilder()
		{
			CloudStorageAccountIsDev();

			_builder.RegisterType<CloudSettingsProvider>().As<IProfileSettings, ISettingsProvider>().SingleInstance();
			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<CloudClient>().SingleInstance();
		}

		public CloudClientBuilder CloudStorageAccountIsDev()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		public CloudClientBuilder CloudStorageAccountIsFromConfig(string name)
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

		public CloudClientBuilder Domain(Action<DomainBuildModule> configuration)
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


		public CloudClient BuildFor(string defaultQueue)
		{
			var container = _builder.Build();
			return container.Resolve<CloudClient>(
				TypedParameter.From(defaultQueue));
		}

		public CloudClientBuilder RegisterModule(IModule module)
		{
			_builder.RegisterModule(module);
			return this;
		}
	}
}