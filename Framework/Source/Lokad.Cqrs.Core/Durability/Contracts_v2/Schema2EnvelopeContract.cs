using ProtoBuf;

namespace Lokad.Cqrs.Durability.Contracts_v2
{
	[ProtoContract]
	public sealed class Schema2EnvelopeContract
	{
		[ProtoMember(1)]
		public readonly string MessageId;
		[ProtoMember(2)]
		public readonly Schema2EnvelopeAttributeContract[] EnvelopeAttributes;
		[ProtoMember(3)]
		public readonly Schema2ItemContract[] Items;

		public Schema2EnvelopeContract(string messageId, Schema2EnvelopeAttributeContract[] envelopeAttributes, Schema2ItemContract[] items)
		{
			MessageId = messageId;
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