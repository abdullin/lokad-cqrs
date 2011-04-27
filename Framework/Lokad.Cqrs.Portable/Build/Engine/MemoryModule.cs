using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.MemoryPartition;

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Autofac syntax for configuring Azure storage
    /// </summary>
    public sealed class MemoryModule : BuildSyntaxHelper, IModule
    {

        readonly IList<IModule> _modules = new List<IModule>();

        public MemoryModule AddPartition(string[] queues, Action<ModuleForMemoryPartition> config)
        {
            foreach (var queue in queues)
            {
                Assert(!ContainsQueuePrefix(queue),
                    "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
            }
            var module = new ModuleForMemoryPartition(queues);
            config(module);
            _modules.Add(module);
            return this;
        }

        const string MemoryDefaultQueue = "memory";
        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();

            if (_modules.OfType<MemoryModule>().Any())
            {
                builder.RegisterType<MemoryPartitionFactory>().As
                    <IQueueWriterFactory, IEngineProcess, MemoryPartitionFactory>().
                    SingleInstance();
            }



            builder.Update(componentRegistry);
        }


        public MemoryModule AddPartition(params string[] queues)
        {
            return AddPartition(queues, m => { });
        }



        public MemoryModule AddPartition(string queueName, Action<ModuleForMemoryPartition> config)
        {
            return AddPartition(new string[] { queueName }, config);
        }

        public MemoryModule AddRouter(string queueName, Func<MessageEnvelope, string> config)
        {
            return AddPartition(queueName, m => m.Dispatch<DispatchMessagesToRoute>(x => x.SpecifyRouter(config)));
        }
    }
}