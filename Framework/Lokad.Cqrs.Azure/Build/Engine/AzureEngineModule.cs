#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Core;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Autofac syntax for configuring Azure storage
    /// </summary>
    public sealed class AzureEngineModule : HideObjectMembersFromIntelliSense, IModule
    {
        static readonly Regex QueueName = new Regex("^[A-Za-z][A-Za-z0-9]{2,62}", RegexOptions.Compiled);

        Action<IComponentRegistry> _funqlets = registry => { };
        
        

        
        public void AddAzureSender(IAzureStorageConfig config, string queueName, Action<SendMessageModule> configure)
        {
            var module = new SendMessageModule((context, endpoint) => new AzureQueueWriterFactory(config, context.Resolve<IEnvelopeStreamer>()), config.AccountName, queueName);
            configure(module);
            _funqlets += module.Configure;
        }

        public void AddAzureSender(IAzureStorageConfig config, string queueName)
        {
            AddAzureSender(config, queueName, m => { });
        }

        public void AddAzureProcess(IAzureStorageConfig config, string[] queues, Action<AzurePartitionModule> configure)
        {
            foreach (var queue in queues)
            {
                if (queue.Contains(":"))
                {
                    var message = string.Format("Queue '{0}' should not contain queue prefix, since it's azure already", queue);
                    throw new InvalidOperationException(message);
                }

                if (!QueueName.IsMatch(queue))
                {
                    var format = string.Format("Queue name should match regex '{0}'", QueueName);
                    throw new InvalidOperationException(format);
                }
            }

            var module = new AzurePartitionModule(config, queues);
            configure(module);
            _funqlets += module.Configure;
        }


        public void AddAzureProcess(IAzureStorageConfig config, string firstQueue, params string[] otherQueues)
        {
            var queues = Enumerable.Repeat(firstQueue, 1).Concat(otherQueues).ToArray();

            AddAzureProcess(config, queues, m => { });
        }

        public void AddAzureProcess(IAzureStorageConfig config, string firstQueue, Action<AzurePartitionModule> configure)
        {
            AddAzureProcess(config, new[] { firstQueue}, configure);
        }

        public void AddAzureRouter(IAzureStorageConfig config, string queueName, Func<ImmutableEnvelope, string> configure)
        {
            AddAzureProcess(config, new[] {queueName}, m => m.DispatchToRoute(configure));
        }


        public void Configure(IComponentRegistry container)
        {
            _funqlets(container);
        }
    }
}