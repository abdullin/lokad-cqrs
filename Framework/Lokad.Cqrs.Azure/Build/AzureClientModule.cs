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
        Action<IComponentRegistry> _modules = context => { };




        public void AddAzureSender(IAzureStorageConfiguration config, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule((context, endpoint) => new AzureQueueWriterFactory(config, context.Resolve<IEnvelopeStreamer>()), config.AccountName, queueName);
            configure(module);
            _modules += module.Configure;
        }

        

        public void AddAzureSender(IAzureStorageConfiguration config, string queueName)
        {
            AddAzureSender(config, queueName, m => { });
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Configure(IComponentRegistry container)
        {
            _modules(container);
        }
    }
}