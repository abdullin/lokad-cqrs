#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Autofac.Core;
using Lokad.Cqrs.Core.Outbox;
using Lokad.Cqrs.Feature.MemoryPartition;

namespace Lokad.Cqrs.Build.Engine
{
    /// <summary>
    /// Autofac syntax for configuring Azure storage
    /// </summary>
    public sealed class MemoryModule : HideObjectMembersFromIntelliSense, IModule
    {
        Action<IComponentRegistry> _funqlets = registry => { };

        public void AddMemoryProcess(string[] queues, Action<MemoryPartitionModule> config)
        {
            foreach (var queue in queues)
            {
                if (queue.Contains(":"))
                {
                    var message = string.Format(
                        "Queue '{0}' should not contain queue prefix, since it's memory already", queue);
                    throw new InvalidOperationException(message);
                }
            }
            var module = new MemoryPartitionModule(queues);
            config(module);
            _funqlets += module.Configure;
        }

        public void Configure(IComponentRegistry componentRegistry)
        {
            _funqlets(componentRegistry);
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
            _funqlets += module.Configure;
        }


        public void AddMemoryProcess(string queueName, Action<MemoryPartitionModule> config)
        {
            AddMemoryProcess(new[] {queueName}, config);
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