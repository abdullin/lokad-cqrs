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

        public void AddMemoryProcess(string[] queues, Action<ModuleForMemoryPartition> config)
        {
            foreach (var queue in queues)
            {
                Assert(!ContainsQueuePrefix(queue),
                    "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
            }
            var module = new ModuleForMemoryPartition(queues);
            config(module);
            _modules.Add(module);
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


        public void AddMemoryProcess(params string[] queues)
        {
            AddMemoryProcess(queues, m => { });
        }

        public void AddMemorySender(string queueName)
        {
            _modules.Add(new SendMessageModule("memory", queueName));
        }



        public void AddMemoryProcess(string queueName, Action<ModuleForMemoryPartition> config)
        {
            AddMemoryProcess(new string[] { queueName }, config);
        }

        public void AddMemoryRouter(string queueName, Func<MessageEnvelope, string> config)
        {
            AddMemoryProcess(queueName, m => m.Dispatch<DispatchMessagesToRoute>(x => x.SpecifyRouter(config)));
        }
    }
}