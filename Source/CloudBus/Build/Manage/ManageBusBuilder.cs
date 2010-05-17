#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using CloudBus.Domain.Build;
using CloudBus.Queue;
using CloudBus.Transport;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;

namespace CloudBus.Build.Manage
{
	public sealed class ManageBusBuilder
	{
		Action<ContainerBuilder> _actions = builder => { };


		public ManageBusBuilder()
		{
			UseDevStoreAccount();
			_actions += builder =>
				{
					builder.RegisterInstance(TraceLog.Provider).SingleInstance();
					builder.RegisterInstance(NullBusProfiler.Instance);
					builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
					builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
				};
		}

		public ManageBusBuilder UseDevStoreAccount()
		{
			_actions += b => b.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		public ManageBusBuilder Domain(Action<DomainBuildModule> configuration)
		{
			ConfigureWith(configuration);
			return this;
		}

		void ConfigureWith<TModule>(Action<TModule> config)
			where TModule : IModule, new()
		{
			var module = new TModule();
			config(module);
			_actions += builder => builder.RegisterModule(module);
		}

		public ManageBusBuilder WithCustom(Action<ContainerBuilder> builder)
		{
			_actions += builder;
			return this;
		}

		public void Apply(ContainerBuilder builder)
		{
			_actions(builder);
			_actions = containerBuilder => { };
		}
	}
}