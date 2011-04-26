#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Core.Envelope
{
    [DataContract, Serializable]
    public sealed class ItemContract
    {
        [DataMember(Order = 1)] public readonly string ContractName;
        [DataMember(Order = 2)] public readonly int ContentSize;
        [DataMember(Order = 3)] public ItemAttributeContract[] Attributes;

        ItemContract()
        {
            Attributes = Empty;
        }

        public ItemContract(string contractName, int contentSize, ItemAttributeContract[] attributes)
        {
            ContractName = contractName;
            ContentSize = contentSize;
            Attributes = attributes;
        }

        static readonly ItemAttributeContract[] Empty = new ItemAttributeContract[0];
    }
}