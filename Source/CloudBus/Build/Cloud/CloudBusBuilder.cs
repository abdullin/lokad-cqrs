using System;
using Autofac;
using Autofac.Core;
using Bus2.Consume.Build;
using Bus2.Domain.Build;
using Bus2.Profiling;
using Bus2.PubSub.Build;
using Bus2.Queue;
using Bus2.Scheduled;
using Bus2.Serialization;
using Bus2.Transport;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;

namespace Bus2.Build.Cloud
{
	public class CloudBusBuilder
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudBusBuilder()
		{
			UseDevStoreAccount();
			
			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullBusProfiler.Instance);

			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<DefaultCloudBusHost>().As<ICloudBusHost>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<CloudSettingsProvider>().As<IProvideBusSettings>().SingleInstance();
		}

		public CloudBusBuilder UseDevStoreAccount()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}


		public CloudBusBuilder PublishSubscribe(Action<BuildPubSubModule> action)
		{
			ConfigureWith(action);
			return this;
		}

		public CloudBusBuilder HandleCommands(Action<HandleCommandsModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudBusBuilder RunTasks(Action<ScheduledModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudBusBuilder HandleEvents(Action<HandleEventsModule> config)
		{
			ConfigureWith(config);
			return this;
		}

		public CloudBusBuilder Domain(Action<DomainBuildModule> configuration)
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