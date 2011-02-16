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
	public sealed class CloudClientBuilder : Syntax
	{

		public CloudClientBuilder()
		{
			Azure.UseDevelopmentStorageAccount();
			Serialization.UseDataContractSerializer();
			Logging.LogToTrace();
			Builder.RegisterInstance(SimpleMessageProfiler.Instance);
			Builder.RegisterInstance(NullEngineProfiler.Instance);
			Builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			Builder.RegisterType<AzureQueueTransport>();
			Builder.RegisterType<CloudClient>().SingleInstance();
		}


		public AutofacBuilderForLogging Logging
		{
			get { return new AutofacBuilderForLogging(Builder); }
		}

		public AutofacBuilderForSerialization Serialization
		{
			get { return new AutofacBuilderForSerialization(Builder); }
		}

		public AutofacBuilderForAzure Azure
		{
			get { return new AutofacBuilderForAzure(Builder); }
		}

		

		/// <summary>
		/// Creates default message sender for the instance of <see cref="ICloudClient"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudClientBuilder AddMessageClient(Action<SenderModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="ICloudClient"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudClientBuilder Domain(Action<DomainBuildModule> config)
		{
			RegisterModule(config);
			return this;
		}

		
		public CloudClient BuildFor(string queueName)
		{
			var container = Builder.Build();

			var lazy = new Lazy<IMessageClient>(() =>
				{
					var queue = container.Resolve<IQueueManager>().GetWriteQueue(queueName);
					return new DefaultMessageClient(queue);
				},false);

			
			return container.Resolve<CloudClient>(TypedParameter.From(lazy));
		}
		
		public CloudClient Build()
		{
			return Builder.Build().Resolve<CloudClient>();
		}
	}
}