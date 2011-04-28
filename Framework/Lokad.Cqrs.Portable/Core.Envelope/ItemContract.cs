#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Core.Envelope
{
    [DataContract(Namespace = "Lokad.Cqrs.v2"), Serializable]
    public sealed class ItemContract
    {
        [DataMember(Order = 1)] public readonly string ContractName;
        [DataMember(Order = 2)] public readonly long ContentSize;
        [DataMember(Order = 3)] public readonly long ContentPosition;
        [DataMember(Order = 4)] public ItemAttributeContract[] Attributes;

        ItemContract()
        {
            Attributes = Empty;
        }

        public ItemContract(string contractName, long contentSize, long contentPosition, ItemAttributeContract[] attributes)
        {
            ContractName = contractName;
            ContentSize = contentSize;
            ContentPosition = contentPosition;
            Attributes = attributes;
        }

        static readonly ItemAttributeContract[] Empty = new ItemAttributeContract[0];
    }
}