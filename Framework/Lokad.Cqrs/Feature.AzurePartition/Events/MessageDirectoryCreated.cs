#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Core.Directory;

namespace Lokad.Cqrs.Feature.AzurePartition.Events
{
    public sealed class MessageDirectoryCreated : ISystemEvent
    {
        public string Origin { get; private set; }
        public MessageInfo[] Messages { get; private set; }
        public ConsumerInfo[] Consumers { get; private set; }

        public MessageDirectoryCreated(string origin, MessageInfo[] messages, ConsumerInfo[] consumers)
        {
            Origin = origin;
            Messages = messages;
            Consumers = consumers;
        }
    }
}