#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac;
using Lokad.Cqrs.Core.Directory;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Core.Reactive;
using Lokad.Cqrs.Core.Serialization;
using Lokad.Cqrs.Feature.AzurePartition.Sender;
using Lokad.Cqrs.Feature.MemoryPartition;

namespace Lokad.Cqrs.Build.Client
{
    /// <summary>
    /// Fluent API for creating and configuring <see cref="CloudClient"/>
    /// </summary>
// ReSharper disable UnusedMember.Global
    public sealed class CloudClientBuilder : BuildSyntaxHelper
    {
        public readonly ContainerBuilder Builder = new ContainerBuilder();

        public CloudClientBuilder()
        {
            // default serialization


            Serialization(x => x.AutoDetectSerializer());

            Builder.RegisterType<ReactiveSystemObserverAdapter>().SingleInstance().As<ISystemObserver>();

            Builder.RegisterType<AzureWriteQueueFactory>().As<IQueueWriterFactory>().SingleInstance();
            Builder.RegisterType<MemoryPartitionFactory>().As<IQueueWriterFactory>().SingleInstance();


            Builder.RegisterType<CloudClient>().SingleInstance();
        }


        /// <summary>
        /// Creates default message sender for the instance of <see cref="CloudClient"/>
        /// </summary>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CloudClientBuilder AddMessageClient(string queueName, string endpointName)
        {
            Builder.RegisterModule(new SendMessageModule(queueName, endpointName));
            return this;
        }

        /// <summary>
        /// Configures the message domain for the instance of <see cref="CloudClient"/>.
        /// </summary>
        /// <param name="config">configuration syntax.</param>
        /// <returns>same builder for inline multiple configuration statements</returns>
        public CloudClientBuilder Domain(Action<ModuleForMessageDirectory> config)
        {
            var m = new ModuleForMessageDirectory();
            config(m);
            Builder.RegisterModule(m);
            return this;
        }

        public CloudClientBuilder Azure(Action<AzureModule> config)
        {
            var m = new AzureModule();
            config(m);
            Builder.RegisterModule(m);
            return this;
        }

        public CloudClientBuilder Serialization(Action<ModuleForSerialization> config)
        {
            var m = new ModuleForSerialization();
            config(m);
            Builder.RegisterModule(m);
            return this;
        }

        public CloudClient Build()
        {
            return Builder.Build().Resolve<CloudClient>();
        }
    }
}