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
	/// <summary>
	/// Configures management environment for the Lokad.CQRS
	/// </summary>
	public sealed class CloudManagerBuilder : ISyntax<ContainerBuilder>
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();


		public CloudManagerBuilder()
		{
			CloudStorageAccountIsDev();

			_builder.RegisterInstance(TraceLog.Provider).SingleInstance();
			_builder.RegisterInstance(NullEngineProfiler.Instance);
			_builder.RegisterType<AzureQueueFactory>().As<IRouteMessages, IQueueManager>().SingleInstance();
			_builder.RegisterType<AzureQueueTransport>().As<IMessageTransport>();
		}

		/// <summary>
		/// Uses default Development storage account for Windows Azure
		/// </summary>
		/// <returns>same builder for inling multiple configuration statements</returns>
		/// <remarks>This option is enabled by default</remarks>
		public CloudManagerBuilder CloudStorageAccountIsDev()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			return this;
		}

		/// <summary>
		/// Uses development storage account from the string being passed.
		/// </summary>
		/// <param name="value">The account string to parse.</param>
		/// <returns>same builder for inling multiple configuration statements</returns>
		public CloudManagerBuilder CloudStorageAccountIsFromString(string value)
		{
			var account = CloudStorageAccount.Parse(value);
			_builder.RegisterInstance(account);
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
			_builder.RegisterModule(module);
		}

		public CloudManagerBuilder WithCustom(Action<ContainerBuilder> builder)
		{
			builder(_builder);
			return this;
		}

		public ContainerBuilder Target
		{
			get { return _builder; }
		}
	}
}