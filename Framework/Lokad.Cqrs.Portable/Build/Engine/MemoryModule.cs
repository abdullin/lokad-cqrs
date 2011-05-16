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
                if (queue.Contains(":"))
                {
                    var message = string.Format("Queue '{0}' should not contain queue prefix, since it's memory already", queue);
                    throw new InvalidOperationException(message);
                }
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
            AddMemorySender(queueName, module => { });
        }

        public void AddMemorySender(string queueName, Action<SendMessageModule> config)
        {
            var module = new SendMessageModule("memory", queueName);
            config(module);
            _modules.Add(module);
        }



        public void AddMemoryProcess(string queueName, Action<ModuleForMemoryPartition> config)
        {
            AddMemoryProcess(new[] { queueName }, config);
        }

        public void AddMemoryRouter(string queueName, Func<ImmutableEnvelope, string> config)
        {
            AddMemoryProcess(queueName, m => m.DispatchToRoute(config));
        }
        public void AddMemoryRouter(string[] queueNames, Func<ImmutableEnvelope, string> config)
        {
            AddMemoryProcess(queueNames, m => m.DispatchToRoute(config));
        }
    }
}