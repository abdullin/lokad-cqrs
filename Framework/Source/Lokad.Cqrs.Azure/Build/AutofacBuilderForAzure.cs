#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Net;
using Autofac;
using Lokad.Cqrs.Storage;
using Lokad.Quality;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs
{
	public sealed class AutofacBuilderForAzure : Syntax
	{
		readonly ContainerBuilder _builder;

		public AutofacBuilderForAzure(ContainerBuilder builder)
		{
			_builder = builder;
			
		}

		/// <summary>
		/// Uses development storage account defined in the string.
		/// </summary>
		/// <param name="accountString">The account string.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		/// <seealso cref="CloudStorageAccount.Parse"/>
		[UsedImplicitly]
		public AutofacBuilderForAzure UseStorageAccount(string accountString)
		{
			var account = CloudStorageAccount.Parse(accountString);
			_builder.RegisterInstance(account);
			DisableNagleForQueuesAndTables(account);
			RegisterLocals();
			return this;
		}

		/// <summary>
		/// Registers the specified storage account as default into the container
		/// </summary>
		/// <param name="account">The account.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		[UsedImplicitly]
		public AutofacBuilderForAzure UseStorageAccount(CloudStorageAccount account)
		{
			_builder.RegisterInstance(account);
			DisableNagleForQueuesAndTables(account);
			RegisterLocals();
			return this;
		}


		/// <summary>
		/// Uses storage account defined in the string.
		/// </summary>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		[UsedImplicitly]
		public AutofacBuilderForAzure UseStorageAccount(string accountName, string accessKey, bool useHttps = true)
		{
			var credentials = new StorageCredentialsAccountAndKey(accountName, accessKey);
			var account = new CloudStorageAccount(credentials, useHttps);
			_builder.RegisterInstance(account);
			DisableNagleForQueuesAndTables(account);
			RegisterLocals();
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
		public AutofacBuilderForAzure UseDevelopmentStorageAccount()
		{
			_builder.RegisterInstance(CloudStorageAccount.DevelopmentStorageAccount);
			RegisterLocals();
			return this;
		}

		/// <summary>
		/// Uses development storage account defined in the configuration setting.
		/// </summary>
		/// <param name="name">The name of the configuration value to look up.</param>
		/// <returns>
		/// same builder for inling multiple configuration statements
		/// </returns>
		public AutofacBuilderForAzure LoadStorageAccountFromSettings([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");


			_builder.Register(c =>
				{
					var value = c.Resolve<IProfileSettings>()
						.GetString(name)
						.ExposeException("Failed to load account from '{0}'", name);
					var account = CloudStorageAccount.Parse(value);
					DisableNagleForQueuesAndTables(account);
					return account;
				}).SingleInstance();

			RegisterLocals();

			return this;
		}

		void RegisterLocals()
		{
			_builder.RegisterType<BlobStorageRoot>().SingleInstance().As<IStorageRoot>();
			_builder.Register(c => c.Resolve<CloudStorageAccount>().CreateCloudBlobClient());
		}
	}
}