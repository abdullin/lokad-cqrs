#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
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

        public MessageActivationInfo[] BuildActivationMap(Func<MessageMapping, bool> filter)
        {
            return _mappings
                .Where(filter)
                .GroupBy(x => x.Message)
                .Select(x => new MessageActivationInfo
                    {
                        MessageType = x.Key,
                        AllConsumers = x
                            .Where(t => t.Consumer != typeof (MessageMapping.BusNull))
                            .Select(m => m.Consumer)
                            .Distinct()
                            .ToArray(),
                    })
                .ToArray();
        }
    }
}