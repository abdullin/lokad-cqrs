#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Serialization;
using Lokad.Cqrs.Transport;
using Lokad.Diagnostics;
using Lokad.Settings;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Fluent API for creating and configuring <see cref="ICloudClient"/>
	/// </summary>
	public sealed class CloudClientBuilder : ISyntax<ContainerBuilder>
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		public CloudClientBuilder()
		{
			this.CloudStorageAccountIsDev();

			_builder.RegisterType<CloudSettingsProvider>().As<IProfileSettings, ISettingsProvider>().SingleInstance();
			_builder.RegisterInstance(TraceLog.Provider);
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<BinaryMessageSerializer>().As<IMessageSerializer>().SingleInstance();
			_builder.RegisterType<CloudClient>().SingleInstance();
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="ICloudEngineHost"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudClientBuilder Domain(Action<DomainBuildModule> config)
		{
			return this.WithModule(config);
		}

		public CloudClient BuildFor(string defaultQueue)
		{
			var container = _builder.Build();
			return container.Resolve<CloudClient>(
				TypedParameter.From(defaultQueue));
		}

		ContainerBuilder ISyntax<ContainerBuilder>.Target
		{
			get { return _builder; }
		}
	}
}