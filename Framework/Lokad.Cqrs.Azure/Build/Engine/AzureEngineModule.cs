#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Microsoft.WindowsAzure;

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

        readonly AzureStorageRegistry _configs = new AzureStorageRegistry();

        readonly IList<IModule> _modules = new List<IModule>();

        

        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="configs">The configs.</param>
        public void AddAzureAccount(params IAzureStorageConfiguration[] configs)
        {
            foreach (var config in configs)
            {
                _configs.Register(config);
            }
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

        public void AddAzureProcess(string accountId, string[] queues, Action<AzurePartitionModule> config)
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

            var module = new AzurePartitionModule(accountId, queues);
            config(module);
            _modules.Add(module);
        }


        public void AddAzureProcess(string accountId, string firstQueue, params string[] otherQueues)
        {
            var queues = Enumerable.Repeat(firstQueue, 1).Concat(otherQueues).ToArray();

            AddAzureProcess(accountId, queues, m => { });
        }

        public void AddAzureProcess(string accountId, string firstQueue, Action<AzurePartitionModule> config)
        {
            AddAzureProcess(accountId, new[] { firstQueue}, config);
        }

        public void AddAzureRouter(string accountId, string queueName, Func<ImmutableEnvelope, string> config)
        {
            AddAzureProcess(accountId, new[] {queueName}, m => m.DispatchToRoute(config));
        }


        public void Configure(IComponentRegistry componentRegistry)
        {
            _configs.Register(AzureStorage.CreateConfigurationForDev());
            var builder = new ContainerBuilder();

            builder.RegisterInstance(_configs);
            foreach (var config in _configs.GetAll())
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