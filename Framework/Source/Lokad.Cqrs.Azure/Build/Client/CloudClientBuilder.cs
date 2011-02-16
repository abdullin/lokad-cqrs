#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Domain;

using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Sender;
using Lokad.Cqrs.Transport;

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
			Azure.UseDevelopmentStorageAccount();
			Serialization.UseDataContractSerializer();
			Logging.LogToTrace();

			_builder.RegisterInstance(SimpleMessageProfiler.Instance);
			_builder.RegisterInstance(NullEngineProfiler.Instance);

			_builder.RegisterType<CloudSettingsProvider>().As<ISettingsProvider>().SingleInstance();
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
			_builder.RegisterType<CloudSettingsProvider>().As<ISettingsProvider>().SingleInstance();
			_builder.RegisterType<CloudClient>().SingleInstance();
		}


		public AutofacBuilderForLogging Logging
		{
			get { return new AutofacBuilderForLogging(_builder); }
		}

		public AutofacBuilderForSerialization Serialization
		{
			get { return new AutofacBuilderForSerialization(_builder); }
		}

		public AutofacBuilderForAzure Azure
		{
			get { return new AutofacBuilderForAzure(_builder); }
		}

		public ContainerBuilder Target
		{
			get { return _builder; }
		}

		/// <summary>
		/// Creates default message sender for the instance of <see cref="ICloudClient"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudClientBuilder AddMessageClient(Action<SenderModule> config)
		{
			return this.WithModule(config);
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="ICloudClient"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudClientBuilder Domain(Action<DomainBuildModule> config)
		{
			return this.WithModule(config);
		}

		
		public CloudClient BuildFor(string queueName)
		{
			var container = _builder.Build();

			var lazy = new Lazy<IMessageClient>(() =>
				{
					var queue = container.Resolve<IQueueManager>().GetWriteQueue(queueName);
					return new DefaultMessageClient(queue);
				},false);

			
			return container.Resolve<CloudClient>(TypedParameter.From(lazy));
		}
		
		public CloudClient Build()
		{
			return _builder.Build().Resolve<CloudClient>();
		}
	}
}