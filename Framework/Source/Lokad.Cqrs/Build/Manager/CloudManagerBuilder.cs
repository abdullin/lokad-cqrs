#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Transport;
using Lokad.Diagnostics;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Configures management environment for the Lokad.CQRS
	/// </summary>
	public sealed class CloudManagerBuilder : ISyntax<ContainerBuilder>
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudManagerBuilder()
		{
			this.CloudStorageAccountIsDev();

			_builder.RegisterInstance(TraceLog.Provider).SingleInstance();
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
		}

		public CloudManagerBuilder Domain(Action<DomainBuildModule> configuration)
		{
			return this.WithModule(configuration);
		}

		ContainerBuilder ISyntax<ContainerBuilder>.Target
		{
			get { return _builder; }
		}

		public IContainer Build()
		{
			return _builder.Build();
		}
	}
}