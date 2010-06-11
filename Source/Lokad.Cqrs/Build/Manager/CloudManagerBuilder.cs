#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Transport;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs
{
	public sealed class CloudManagerBuilder
	{
		Action<ContainerBuilder> _actions = builder => { };


		public CloudManagerBuilder()
		{
			CloudStorageAccountIsDev();
			_actions += builder =>
				{
					builder.RegisterInstance(TraceLog.Provider).SingleInstance();
					builder.RegisterInstance(NullEngineProfiler.Instance);
					builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
					builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
				};
		}

		public CloudManagerBuilder CloudStorageAccountIsDev()
		{
			_actions += b => b.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		public CloudManagerBuilder CloudStorageAccountIsFromString(string value)
		{
			var account = CloudStorageAccount.Parse(value);
			_actions += b => b.RegisterInstance(account);
			return this;
		}

		public CloudManagerBuilder Domain(Action<DomainBuildModule> configuration)
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

		public CloudManagerBuilder WithCustom(Action<ContainerBuilder> builder)
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