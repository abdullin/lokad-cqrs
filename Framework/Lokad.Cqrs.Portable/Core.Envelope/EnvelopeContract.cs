#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Core.Envelope
{
    [DataContract(Namespace = "Lokad.Cqrs.v2", Name = "Envelope"), Serializable]
    public sealed class EnvelopeContract
    {
        [DataMember(Order = 1)] public readonly string EnvelopeId;
        [DataMember(Order = 2)] public readonly EnvelopeAttributeContract[] EnvelopeAttributes;
        [DataMember(Order = 3)] public readonly MessageContract[] Messages;
        [DataMember(Order = 4)] public readonly DateTime DeliverOnUtc;
        [DataMember(Order = 5)] public readonly DateTime CreatedOnUtc;

        public EnvelopeContract(string envelopeId, EnvelopeAttributeContract[] envelopeAttributes, MessageContract[] messages,
            DateTime deliverOnUtc, DateTime createdOnUtc)
        {
            EnvelopeId = envelopeId;
            DeliverOnUtc = deliverOnUtc;
            EnvelopeAttributes = envelopeAttributes;
            Messages = messages;
            CreatedOnUtc = createdOnUtc;
        }

// ReSharper disable UnusedMember.Local
        EnvelopeContract()
// ReSharper restore UnusedMember.Local
        {
            Messages = NoMessages;
            EnvelopeAttributes = NoAttributes;
        }

        static readonly MessageContract[] NoMessages = new MessageContract[0];
        static readonly EnvelopeAttributeContract[] NoAttributes = new EnvelopeAttributeContract[0];
    }
}