#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Core.Envelope
{
    [DataContract, Serializable]
    public sealed class EnvelopeContract
    {
        [DataMember(Order = 1)] public readonly string EnvelopeId;
        [DataMember(Order = 2)] public readonly EnvelopeAttributeContract[] EnvelopeAttributes;
        [DataMember(Order = 3)] public readonly ItemContract[] Items;

        [DataMember(Order = 4)] public readonly DateTimeOffset DeliverOnUtc;

        public EnvelopeContract(string envelopeId, EnvelopeAttributeContract[] envelopeAttributes, ItemContract[] items,
            DateTimeOffset deliverOnUtc)
        {
            EnvelopeId = envelopeId;
            DeliverOnUtc = deliverOnUtc;
            EnvelopeAttributes = envelopeAttributes;
            Items = items;
        }

// ReSharper disable UnusedMember.Local
        EnvelopeContract()
// ReSharper restore UnusedMember.Local
        {
            Items = NoItems;
            EnvelopeAttributes = NoAttributes;
        }

        static readonly ItemContract[] NoItems = new ItemContract[0];
        static readonly EnvelopeAttributeContract[] NoAttributes = new EnvelopeAttributeContract[0];
    }
}