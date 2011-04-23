#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Net;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Feature.StreamingStorage;
using Lokad.Cqrs.Feature.StreamingStorage.Azure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Lokad.Cqrs.Build
{
	/// <summary>
	/// Autofac syntax for configuring Azure storage
	/// </summary>
	public sealed class ModuleForAzure : BuildSyntaxHelper, IModule
	{
		readonly ContainerBuilder _builder = new ContainerBuilder();

		Action<CloudBlobClient> _configureBlobClient = client => { };


		/// <summary>
		/// Uses development storage account defined in the string.
		/// </summary>
		/// <param name="accountString">The account string.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		/// <seealso cref="CloudStorageAccount.Parse"/>
		
		public ModuleForAzure UseStorageAccount(string accountString)
		{
			var account = CloudStorageAccount.Parse(accountString);
			_builder.RegisterInstance(account);
			DisableNagleForQueuesAndTables(account);
			return this;
		}

		/// <summary>
		/// Registers the specified storage account as default into the container
		/// </summary>
		/// <param name="account">The account.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		
		public ModuleForAzure UseStorageAccount(CloudStorageAccount account)
		{
			_builder.RegisterInstance(account);
			DisableNagleForQueuesAndTables(account);
			return this;
		}


		/// <summary>
		/// Uses storage account defined in the string.
		/// </summary>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		
		public ModuleForAzure UseStorageAccount(string accountName, string accessKey, bool useHttps = true)
		{
			var credentials = new StorageCredentialsAccountAndKey(accountName, accessKey);
			var account = new CloudStorageAccount(credentials, useHttps);
			_builder.RegisterInstance(account);
			DisableNagleForQueuesAndTables(account);
			return this;
		}

		static void DisableNagleForQueuesAndTables(CloudStorageAccount account)
		{
			// http://blogs.msdn.com/b/windowsazurestorage/archive/2010/06/25/nagle-s-algorithm-is-not-friendly-towards-small-requests.aspx
			// improving transfer speeds for the small requests
			ServicePointManager.FindServicePoint(account.TableEndpoint).UseNagleAlgorithm = false;
			ServicePointManager.FindServicePoint(account.QueueEndpoint).UseNagleAlgorithm = false;
		}


		/// <summary>
		/// Uses default Development storage account for Windows Azure
		/// </summary>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		/// <remarks>This option is enabled by default</remarks>
		public ModuleForAzure UseDevelopmentStorageAccount()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			
			return this;
		}

		public ModuleForAzure AddPartition(string[] queues, Action<ModuleForAzurePartition> config)
		{
			foreach (var queue in queues)
			{
				Assert(!ContainsQueuePrefix(queue), "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
			}
			var module = new ModuleForAzurePartition(queues);

			config(module);
			_builder.RegisterModule(module);
			return this;
		}

		public ModuleForAzure AddPartition(params string[] queues)
		{
			return AddPartition(queues, m => { });
		}

		/// <summary>
		/// Uses development storage as retrieved from the provider
		/// </summary>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		public ModuleForAzure LoadStorageAccountFromSettings(Func<IComponentContext, string> configProvider)
		{
			_builder.Register(c =>
				{
					var value = configProvider(c);
					var account = CloudStorageAccount.Parse(value);
					DisableNagleForQueuesAndTables(account);
					return account;
				}).SingleInstance();


			return this;
		}

		/// <summary>
		/// Uses development storage as retrieved from the provider
		/// </summary>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		public ModuleForAzure LoadStorageAccountFromSettings(Func<string> configProvider)
		{
			return LoadStorageAccountFromSettings(c => configProvider());
		}



		/// <summary>
		/// Configures the BLOB client. This action is applied to every single instance created.
		/// </summary>
		/// <param name="action">The action.</param>
		public void ConfigureBlobClient(Action<CloudBlobClient> action)
		{
			_configureBlobClient += action;
		}

		public void Configure(IComponentRegistry componentRegistry)
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			_builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
			_builder.RegisterType<AzurePartitionFactory>().As<AzurePartitionFactory, IEngineProcess>().SingleInstance();

			_builder.RegisterType<BlobStorageRoot>().SingleInstance().As<IStorageRoot>();
			_builder.Register(c =>
			{
				var client = c.Resolve<CloudStorageAccount>().CreateCloudBlobClient();
				_configureBlobClient(client);
				return client;
			});

			_builder.Update(componentRegistry);
		}


		
	}
}