using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Autofac;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class QueueWriterRegistry
    {

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        readonly ConcurrentDictionary<string, IQueueWriterFactory> _instances = new ConcurrentDictionary<string, IQueueWriterFactory>();

        readonly ConcurrentDictionary<string,Func<string,IQueueWriterFactory>> _registrations = new ConcurrentDictionary<string, Func<string, IQueueWriterFactory>>();


        public void Add(IQueueWriterFactory factory)
        {
            if (!_instances.TryAdd(factory.Endpoint, factory))
            {
                var message = string.Format("Failed to add {0}.{1}", factory.GetType().Name, factory.Endpoint);
                throw new InvalidOperationException(message);
            }
        }

        public void AddActivator(QueueWriterActivator entry)
        {
            if(!_registrations.TryAdd(entry.EndpointName, entry.Activator))
            {
                throw new InvalidOperationException(string.Format("Failed to add registration for '{0}'", entry.Activator));
            }
        }

        public IQueueWriterFactory Get(string endpoint)
        {
            IQueueWriterFactory factory;
            if (!TryGet(endpoint,out factory))
            {
                string message = string.Format("Failed to retrieve factory for '{0}'", endpoint);
                throw new InvalidOperationException(message);
            }
            return factory;
        }


        public bool TryGet(string endpoint, out IQueueWriterFactory factory)
        {
            if (_instances.TryGetValue(endpoint, out factory))
                return true;
            Func<string, IQueueWriterFactory> func;
            if (_registrations.TryGetValue(endpoint, out func))
            {
                factory = _instances.GetOrAdd(endpoint, func);
                return true;
            }

            factory = null;
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}], x{2:X8}", this.GetType().Name, _instances.Count, GetHashCode());
        }
    }

    public sealed class QueueWriterActivator
    {
        public readonly string EndpointName;
        public readonly Func<string, IQueueWriterFactory> Activator;

        public QueueWriterActivator(string endpointName, Func<string, IQueueWriterFactory> activator)
        {
            EndpointName = endpointName;
            Activator = activator;
        }
    }
}