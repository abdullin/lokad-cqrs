#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)] readonly ImmutableAttribute[] _attributes;
        
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public readonly ImmutableMessage[] Items;

        public ImmutableEnvelope(string envelopeId, ImmutableAttribute[] attributes, ImmutableMessage[] items,
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
            return _attributes.First(n => n.Key == name).Value;
        }

        public string GetAttribute(string name, string defaultValue)
        {
            foreach (var attribute in _attributes)
            {
                if (attribute.Key == name)
                    return attribute.Value;
            }
            return defaultValue;
        }




        public ICollection<ImmutableAttribute> GetAllAttributes()
        {
            return _attributes;
        }
    }
}