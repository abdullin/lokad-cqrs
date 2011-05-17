#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Core.Directory
{
    /// <summary>
    /// Default implementation of the message directory builder
    /// </summary>
    public sealed class MessageDirectoryBuilder
    {
        readonly IEnumerable<MessageMapping> _mappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageDirectoryBuilder"/> class.
        /// </summary>
        /// <param name="mappings">The message mappings.</param>
        public MessageDirectoryBuilder(IEnumerable<MessageMapping> mappings)
        {
            _mappings = mappings;
        }

        public ICollection<Type> ListMessagesToSerialize()
        {
            return _mappings
                .Select(x => x.Message)
                .Where(x => !x.IsAbstract)
                .Distinct()
                .ToArray();
        }

        public ICollection<Type> ListConsumersToActivate()
        {
            return _mappings
                .Select(x => x.Consumer)
                .Where(x => !x.IsAbstract)
                .Distinct()
                .ToArray();
        }

        public MessageActivationMap BuildActivationMap(Func<MessageMapping, bool> filter)
        {
            var mappings = _mappings.Where(filter);

            var messages = mappings
                .ToLookup(x => x.Message)
                .Select(x =>
                {
                    var domainConsumers = x
                        .Where(t => t.Consumer != typeof(MessageMapping.BusNull))
                        .ToArray();

                    return new MessageActivationInfo
                        {
                            MessageType = x.Key,
                            AllConsumers = domainConsumers
                                .Select(m => m.Consumer)
                                .Distinct()
                                .ToArray(),
                            //DerivedConsumers =
                            //    domainConsumers.Where(m => !m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
                            //DirectConsumers =
                            //    domainConsumers.Where(m => m.Direct).Select(m => m.Consumer).Distinct().ToArray(),
                        };
                }).ToList();



            
            return new MessageActivationMap(messages);
        }
    }
}