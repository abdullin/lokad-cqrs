#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.MemoryPartition
{
    public sealed class MemoryFutureList
    {
        readonly IList<ImmutableEnvelope> _schedule = new List<ImmutableEnvelope>();
        readonly object _lock = new object();

        public void PutMessage(ImmutableEnvelope envelope)
        {
            lock (_lock)
            {
                _schedule.Add(envelope);
            }
        }

        public bool TakePendingMessage(out ImmutableEnvelope envelope)
        {
            var dateTimeOffset = DateTimeOffset.UtcNow;
            lock (_lock)
            {
                envelope = _schedule
                    .OrderBy(x => x.DeliverOnUtc)
                    .FirstOrDefault(t => t.DeliverOnUtc <= dateTimeOffset);
                if (null != envelope)
                {
                    _schedule.Remove(envelope);
                    return true;
                }
            }
            return false;
        }
    }
}