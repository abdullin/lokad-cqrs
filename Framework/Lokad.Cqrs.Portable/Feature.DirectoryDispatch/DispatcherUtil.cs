#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Feature.DirectoryDispatch
{
    public static class DispatcherUtil
    {
        public static void ThrowIfCommandHasMultipleConsumers(IEnumerable<MessageActivationInfo> commands)
        {
            var multipleConsumers = commands
                .Where(c => c.AllConsumers.Length > 1)
                .Select(c => c.MessageType.FullName)
                .ToArray();

            if (!multipleConsumers.Any())
                return;

            var joined = string.Join("; ", multipleConsumers);

            throw new InvalidOperationException(
                "These messages have multiple consumers. Did you intend to declare them as events? " + joined);
        }
    }
}