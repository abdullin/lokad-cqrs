#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.Send;

namespace Lokad.Cqrs.Build.Client
{
	/// <summary>
	/// Fluent API for creating and configuring <see cref="CloudClient"/>
	/// </summary>
// ReSharper disable UnusedMember.Global
	public sealed class CloudClientBuilder : AutofacBuilderBase
	{
		public CloudClientBuilder()
		{
			Azure.UseDevelopmentStorageAccount();
			Serialization.AutoDetectSerializer();
			Logging.LogToTrace();

			Builder.RegisterType<AzureWriteQueueFactory>().As<IWriteQueueFactory>().SingleInstance();
			

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
		/// Creates default message sender for the instance of <see cref="CloudClient"/>
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudClientBuilder AddMessageClient(Action<SendMessageModule> config)
		{
			RegisterModule(config);
			return this;
		}

		/// <summary>
		/// Configures the message domain for the instance of <see cref="CloudClient"/>.
		/// </summary>
		/// <param name="config">configuration syntax.</param>
		/// <returns>same builder for inline multiple configuration statements</returns>
		public CloudClientBuilder Domain(Action<DomainBuildModule> config)
		{
			RegisterModule(config);
			return this;
		}

		
		public CloudClient BuildFor(string queueName)
		{
			var container = Builder.Build();

			var lazy = new Lazy<IMessageSender>(() =>
				{
					var queue = container.Resolve<IWriteQueueFactory>().GetWriteQueue(queueName);
					return new DefaultMessageSender(queue);
				},false);

			
			return container.Resolve<CloudClient>(TypedParameter.From(lazy));
		}
		
		public CloudClient Build()
		{
			return Builder.Build().Resolve<CloudClient>();
		}
	}
}