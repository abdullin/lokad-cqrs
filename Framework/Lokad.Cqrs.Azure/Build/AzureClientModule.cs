using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Microsoft.WindowsAzure;

namespace Lokad.Cqrs.Build
{
    public sealed class AzureClientModule : BuildSyntaxHelper, IModule
    {
        readonly AzureStorageDictionary _dictionary = new AzureStorageDictionary();

        readonly IList<IModule> _modules = new List<IModule>();

        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account">The account.</param>
        /// <returns>
        /// same builder for inling multiple configuration statements
        /// </returns>
        public void AddAzureAccount(string accountId, CloudStorageAccount account,
            Action<AzureStorageConfigurationBuilder> tuning)
        {
            var builder = new AzureStorageConfigurationBuilder(account, accountId);
            tuning(builder);
            var configuration = builder.Build();
            _dictionary.Register(configuration);
        }

        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>
        /// same builder for inling multiple configuration statements
        /// </returns>
        public void AddAzureAccount(string accountId, CloudStorageAccount account)
        {
            AddAzureAccount(accountId, account, builder => { });
        }

        public void AddAzureSender(string accountId, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule(accountId, queueName);
            configure(module);
            _modules.Add(module);
        }

        public void AddAzureSender(string accountId, string queueName)
        {
            AddAzureSender(accountId, queueName, m => { });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();

            if (!_dictionary.Contains("azure-dev"))
            {
                AddAzureAccount("azure-dev", CloudStorageAccount.DevelopmentStorageAccount);
            }

            builder.RegisterInstance(_dictionary);
            foreach (var config in _dictionary.GetAll())
            {
                builder.RegisterInstance(config).Named<IAzureStorageConfiguration>(config.AccountName);
            }


            foreach (var partition in _modules)
            {
                builder.RegisterModule(partition);
            }
            builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
            builder.Update(componentRegistry);
        }
    }
}