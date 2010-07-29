#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Domain.Build;
using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Transport;
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
			Azure.UseDevelopmentStorageAccount();
			Serialization.UseBinaryFormatter();
			Logging.LogToTrace();


			_builder.RegisterType<CloudSettingsProvider>().As<IProfileSettings, ISettingsProvider>().SingleInstance();
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
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

		ContainerBuilder ISyntax<ContainerBuilder>.Target
		{
			get { return _builder; }
		}

		//public CloudClientBuilder LoggingIs(Action<ISupportSyntaxForLogging> configure)
		//{
		//    configure(Logging);
		//    return this;
		//}

		//public CloudClientBuilder SerializationIs(Action<AutofacBuilderForSerialization> configure)
		//{
		//    configure(Serialization);
		//    return this;
		//}

		//public CloudClientBuilder AzureIs(Action<AutofacBuilderForAzure> configure)
		//{
		//    configure(Azure);
		//    return this;
		//}

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
	}
}