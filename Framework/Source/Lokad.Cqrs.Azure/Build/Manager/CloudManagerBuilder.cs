#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Domain;

using Lokad.Cqrs.Queue;
using Lokad.Cqrs.Transport;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Configures management environment for the Lokad.CQRS
	/// </summary>
	
	public sealed class CloudManagerBuilder : Syntax
	{
		public AutofacBuilderForLogging Logging { get { return new AutofacBuilderForLogging(Builder);}}
		public AutofacBuilderForAzure Azure { get { return new AutofacBuilderForAzure(Builder);} }
		public AutofacBuilderForSerialization Serialization { get { return new AutofacBuilderForSerialization(Builder);}}


		public CloudManagerBuilder()
		{
			Logging.LogToTrace();

			
			Builder.RegisterInstance(NullEngineProfiler.Instance);
			Builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			Builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
		}

		public CloudManagerBuilder DomainIs(Action<DomainBuildModule> configuration)
		{
			var m = new DomainBuildModule();
			configuration(m);
			Builder.RegisterModule(m);
			return this;
		}

		public IContainer Build()
		{
			return Builder.Build();
		}
	}
}