using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Lokad.Cqrs.Core.Outbox
{
    public sealed class QueueWriterRegistry
    {
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        readonly ConcurrentDictionary<string, IQueueWriterFactory> _dictionary = new ConcurrentDictionary<string, IQueueWriterFactory>();
        public void Add(IQueueWriterFactory factory)
        {
            if (!_dictionary.TryAdd(factory.Endpoint, factory))
            {
                var message = string.Format("Failed to add {0}.{1}", factory.GetType().Name, factory.Endpoint);
                throw new InvalidOperationException(message);
            }
        }


        public IQueueWriterFactory GetOrAdd(string endpoint, Func<string,IQueueWriterFactory> factory)
        {
            return _dictionary.GetOrAdd(endpoint, factory);
        }

        public bool TryGet(string endpoint, out IQueueWriterFactory factory)
        {
            return _dictionary.TryGetValue(endpoint, out factory);
        }

        public override string ToString()
        {
            return string.Format("{0}[{1}], x{2:X8}", this.GetType().Name, _dictionary.Count, GetHashCode());
        }
    }
}