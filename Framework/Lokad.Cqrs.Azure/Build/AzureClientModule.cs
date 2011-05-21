#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.ComponentModel;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;

namespace Lokad.Cqrs.Build
{
    public sealed class AzureClientModule : HideObjectMembersFromIntelliSense, IModule
    {
        readonly AzureStorageRegistry _dictionary = new AzureStorageRegistry();

        Action<IComponentRegistry> _modules = context => { };

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
            _modules += module.Configure;
        }

        public void AddAzureSender(string accountId, string queueName)
        {
            AddAzureSender(accountId, queueName, m => { });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure(IComponentRegistry container)
        {
            if (!_dictionary.Contains("azure-dev"))
            {
                AddAzureAccount(AzureStorage.CreateConfigurationForDev());
            }

            container.Register(_dictionary);
            container.Register<IQueueWriterFactory>(
                c => new AzureWriteQueueFactory(_dictionary, c.Resolve<IEnvelopeStreamer>()));

            _modules(container);
        }
    }
}