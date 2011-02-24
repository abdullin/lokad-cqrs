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
	}
}