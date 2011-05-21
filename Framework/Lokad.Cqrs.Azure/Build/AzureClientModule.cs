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
    public sealed class AzureClientModule : HideObjectMembersFromIntelliSense, IModule
    {
        readonly AzureStorageRegistry _dictionary = new AzureStorageRegistry();

        readonly IList<IModule> _modules = new List<IModule>();

        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="storage">The storage.</param>
        public void AddAzureAccount(IAzureStorageConfiguration storage)
        {
            _dictionary.Register(storage);
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
                AddAzureAccount(AzureStorage.CreateConfigurationForDev());
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