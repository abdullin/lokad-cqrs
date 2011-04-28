#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lokad.Cqrs.Core.Envelope;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Deserialized message representation
    /// </summary>
    public class MessageEnvelope
    {
        public readonly string EnvelopeId;
        public DateTimeOffset DeliverOn;
        public readonly DateTimeOffset CreatedOn;
        readonly IDictionary<string, object> _attributes = new Dictionary<string, object>();
        
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public readonly MessageItem[] Items;

        public MessageEnvelope(string envelopeId, IDictionary<string, object> attributes, MessageItem[] items,
            DateTimeOffset deliverOn, DateTimeOffset createdOn)
        {
            EnvelopeId = envelopeId;
            DeliverOn = deliverOn;
            _attributes = attributes;
            Items = items;
            CreatedOn = createdOn;
        }

        
        public object GetAttribute(string name)
        {
            return _attributes[name];
        }

        

        public ICollection<KeyValuePair<string, object>> GetAllAttributes()
        {
            return _attributes;
        }
    }
}