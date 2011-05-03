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
        internal readonly IDictionary<string, string> _attributes = new Dictionary<string, string>();
        
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public readonly ImmutableMessage[] Items;

        public ImmutableEnvelope(string envelopeId, IDictionary<string, string> attributes, ImmutableMessage[] items,
            DateTime deliverOnUtc, DateTime createdOnUtc)
        {
            EnvelopeId = envelopeId;
            DeliverOnUtc = deliverOnUtc;
            _attributes = attributes;
            Items = items;
            CreatedOnUtc = createdOnUtc;
        }

        


        public string GetAttribute(string name)
        {
            return _attributes[name];
        }

        public string GetAttribute(string name, string defaultValue)
        {
            string result;
            if (_attributes.TryGetValue(name, out result))
            {
                return result;
            }
            return defaultValue;
        }


        

        public ICollection<KeyValuePair<string, string>> GetAllAttributes()
        {
            return _attributes;
        }
    }
}