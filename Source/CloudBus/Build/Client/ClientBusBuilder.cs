using System;
using Autofac;
using Autofac.Core;
using Bus2.Domain.Build;
using Bus2.Profiling;
using Bus2.Queue;
using Bus2.Serialization;
using Bus2.Transport;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;

namespace Bus2.Build.Client
{
	public sealed class ClientBusBuilder
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public ClientBusBuilder()
		{
			UseDevStoreAccount();

			_builder.RegisterType<CloudSettingsProvider>().As<IProvideBusSettings>().SingleInstance();
			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullBusProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<ClientBus>().SingleInstance();
		}

		public ClientBusBuilder UseDevStoreAccount()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
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
				TypedParameter.From(container),
				TypedParameter.From(defaultQueue));
		}
	}
}