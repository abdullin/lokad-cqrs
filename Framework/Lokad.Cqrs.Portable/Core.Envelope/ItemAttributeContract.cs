#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Core.Envelope
{
    [DataContract, Serializable]
    public sealed class ItemAttributeContract
    {
        [DataMember(Order = 1)]
        public ItemAttributeTypeContract Type { get; set; }

        [DataMember(Order = 2)]
        public string CustomName { get; set; }

        [DataMember(Order = 3)]
        public string StringValue { get; set; }

        [DataMember(Order = 4)]
        public long NumberValue { get; set; }
    }
}