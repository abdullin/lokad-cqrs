#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Microsoft.WindowsAzure;
using System.Linq;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Lokad.Cqrs.Build
{
    /// <summary>
    /// Autofac syntax for configuring Azure storage
    /// </summary>
    public sealed class AzureModule : BuildSyntaxHelper, IModule
    {

        static readonly Regex QueueName = new Regex("^[A-Za-z][A-Za-z0-9]{2,62}", RegexOptions.Compiled);

        readonly IDictionary<string,AzureClientConfiguration> _configs = new Dictionary<string, AzureClientConfiguration>();
        readonly IList<IModule> _modules = new List<IModule>();



        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account">The account.</param>
        /// <returns>
        /// same builder for inling multiple configuration statements
        /// </returns>
        public void AddAzureAccount(string accountId, CloudStorageAccount account, Action<AzureClientConfigurationBuilder> tuning)
        {
            var builder = new AzureClientConfigurationBuilder(account, accountId);
            tuning(builder);
            var configuration = builder.Build();
            _configs.Add(configuration.AccountName, configuration);
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

        public void AddAzureProcess(string accountId, string[] queues, Action<AzurePartitionModule> config)
        {
            foreach (var queue in queues)
            {
                Assert(!ContainsQueuePrefix(queue),
                    "Queue '{0}' should not contain queue prefix, since it's azure already", queue);

                Assert(QueueName.IsMatch(queue), "Queue name should match regex '{0}'", QueueName.ToString());
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

        public void AddAzureRouter(string accountId, string queueName, Func<ImmutableEnvelope, string> config)
        {
            AddAzureProcess(accountId, new[] {queueName}, m => m.Dispatch<DispatchMessagesToRoute>(x => x.SpecifyRouter(config)));
        }


        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();

            if (!_configs.ContainsKey("azure-dev"))
            {

                AddAzureAccount("azure-dev", CloudStorageAccount.DevelopmentStorageAccount);
            }

            foreach (var config in _configs)
            {
                builder.RegisterInstance(config.Value).As<IAzureClientConfiguration>();
            }

            foreach (var partition in _modules)
            {
                builder.RegisterModule(partition);
            }

            builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
            //builder.RegisterType<AzurePartitionFactory>().As<AzurePartitionFactory, IEngineProcess>().SingleInstance();
            builder.Update(componentRegistry);
        }
    }
}