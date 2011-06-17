using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Scenarios.SimpleES
{
    public sealed class InMemoryEventStreamer<TEvent>
    {
        // non serialized
        readonly ConcurrentDictionary<string,List<TEvent>> _streams = new ConcurrentDictionary<string, List<TEvent>>();
        readonly IMessageSender _sender;
        public InMemoryEventStreamer(IMessageSender sender)
        {
            _sender = sender;
        }

        public void Append(string key, Func<IEnumerable<TEvent>, IEnumerable<TEvent>> append)
        {
            _streams.AddOrUpdate(key, s =>
                {
                    var result = append(Enumerable.Empty<TEvent>());

                    // temporary
                    foreach (var @event in result)
                    {
                        _sender.SendOne(@event, eb =>eb.AddString("EntityId", key));
                    }

                    return new List<TEvent>(result);
                }, (s, list) =>
                    {
                        var result = append(list);

                        // temporary
                        foreach (var @event in result)
                        {
                            _sender.SendOne(@event, eb => eb.AddString("EntityId", key));
                        }
                        list.AddRange(result);
                        return list;
                    });
        }

    }
}