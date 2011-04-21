using System;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport.Contracts_v2
{
	[DataContract]
	public sealed class Schema2EnvelopeContract
	{
		[DataMember(Order = 1)]
		public readonly string EnvelopeId;
		[DataMember(Order = 2)]
		public readonly Schema2EnvelopeAttributeContract[] EnvelopeAttributes;
		[DataMember(Order = 3)]
		public readonly Schema2ItemContract[] Items;

		[DataMember(Order = 4)]
		public readonly DateTimeOffset DeliverOnUtc;

		public Schema2EnvelopeContract(string envelopeId, Schema2EnvelopeAttributeContract[] envelopeAttributes, Schema2ItemContract[] items, DateTimeOffset deliverOnUtc)
		{
			EnvelopeId = envelopeId;
			DeliverOnUtc = deliverOnUtc;
			EnvelopeAttributes = envelopeAttributes;
			Items = items;
		}

// ReSharper disable UnusedMember.Local
		Schema2EnvelopeContract()
// ReSharper restore UnusedMember.Local
		{
			Items = NoItems;
			EnvelopeAttributes = NoAttributes;
			
		}

		static readonly Schema2ItemContract[] NoItems = new Schema2ItemContract[0];
		static readonly Schema2EnvelopeAttributeContract[] NoAttributes = new Schema2EnvelopeAttributeContract[0];
	}
}