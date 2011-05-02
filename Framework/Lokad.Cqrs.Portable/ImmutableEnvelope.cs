#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Deserialized message representation
    /// </summary>
    public class ImmutableEnvelope
    {
        public readonly string EnvelopeId;
        public readonly DateTime DeliverOnUtc;
        public readonly DateTime CreatedOnUtc;
        internal readonly IDictionary<string, object> _attributes = new Dictionary<string, object>();
        
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public readonly ImmutableMessage[] Items;

        public ImmutableEnvelope(string envelopeId, IDictionary<string, object> attributes, ImmutableMessage[] items,
            DateTime deliverOnUtc, DateTime createdOnUtc)
        {
            EnvelopeId = envelopeId;
            DeliverOnUtc = deliverOnUtc;
            _attributes = attributes;
            Items = items;
            CreatedOnUtc = createdOnUtc;
        }

        public long GetAttributeNumber(string name, long defaultValue)
        {
            object value;
            if (_attributes.TryGetValue(name, out value))
            {
                return (long) value;
            }
            return defaultValue;
        }
        public string GetAttributeString(string name, string defaultValue)
        {
            object value;
            if (_attributes.TryGetValue(name, out value))
            {
                return (string)value;
            }
            return defaultValue;
            
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