#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.AzurePartition.Inbox;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
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
        readonly IList<IModule> _partitions = new List<IModule>();



        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account">The account.</param>
        /// <returns>
        /// same builder for inling multiple configuration statements
        /// </returns>
        public AzureModule AddAzureAccount(string accountId, CloudStorageAccount account, Action<AzureClientConfigurationBuilder> tuning)
        {
            var builder = new AzureClientConfigurationBuilder(account, accountId);
            tuning(builder);
            var configuration = builder.Build();
            _configs.Add(configuration.AccountName, configuration);
            return this;
        }

        /// <summary>
        /// Registers the specified storage account as default into the container
        /// </summary>
        /// <param name="account">The account.</param>
        /// <returns>
        /// same builder for inling multiple configuration statements
        /// </returns>
        public AzureModule AddAzureAccount(string accountId, CloudStorageAccount account)
        {
            return AddAzureAccount(accountId, account, builder => { });
        }

        public AzureModule AddAzureSender(string accountId, string queueName)
        {
            var sms = new SendMessageModule(accountId, queueName);
            _partitions.Add(sms);
            return this;
        }


        public AzureModule AddAzurePartition(string accountId, string[] queues, Action<AzurePartitionModule> config)
        {
            foreach (var queue in queues)
            {
                Assert(!ContainsQueuePrefix(queue),
                    "Queue '{0}' should not contain queue prefix, since it's azure already", queue);

                Assert(QueueName.IsMatch(queue), "Queue name should match regex '{0}'", QueueName.ToString());
            }

            var module = new AzurePartitionModule(accountId, queues);
            config(module);
            _partitions.Add(module);
            return this;
        }

        

        public AzureModule AddAzurePartition(string accountId, string firstQueue, params string[] otherQueues)
        {
            var queues = Enumerable.Repeat(firstQueue, 1).Concat(otherQueues).ToArray();
            
            return AddAzurePartition(accountId, queues, m => { });
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

            foreach (var partition in _partitions)
            {
                builder.RegisterModule(partition);
            }

            builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
            //builder.RegisterType<AzurePartitionFactory>().As<AzurePartitionFactory, IEngineProcess>().SingleInstance();
            builder.Update(componentRegistry);
        }
    }
}