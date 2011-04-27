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

        public MemoryModule AddMemoryPartition(string[] queues, Action<ModuleForMemoryPartition> config)
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

        public void Configure(IComponentRegistry componentRegistry)
        {
            var builder = new ContainerBuilder();

            if (_modules.OfType<ModuleForMemoryPartition>().Any())
            {
                builder.RegisterType<MemoryPartitionFactory>().As
                    <IQueueWriterFactory, IEngineProcess, MemoryPartitionFactory>().
                    SingleInstance();
            }
            foreach (var module in _modules)
            {
                builder.RegisterModule(module);
            }



            builder.Update(componentRegistry);
        }


        public MemoryModule AddMemoryPartition(params string[] queues)
        {
            return AddMemoryPartition(queues, m => { });
        }

        public void AddMemorySender(string queueName)
        {
            _modules.Add(new SendMessageModule("memory", queueName));
        }



        public MemoryModule AddMemoryPartition(string queueName, Action<ModuleForMemoryPartition> config)
        {
            return AddMemoryPartition(new string[] { queueName }, config);
        }

        public MemoryModule AddMemoryRouter(string queueName, Func<MessageEnvelope, string> config)
        {
            return AddMemoryPartition(queueName, m => m.Dispatch<DispatchMessagesToRoute>(x => x.SpecifyRouter(config)));
        }

        
    }
}