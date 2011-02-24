using ProtoBuf;

namespace Lokad.Cqrs
{
	[ProtoContract]
	public sealed class Schema2EnvelopeContract
	{
		[ProtoMember(1)]
		public readonly string MessageId;

		public readonly Schema2AttributeContract[] EnvelopeAttributes;

		public readonly Schema2ItemContract[] Items;

		public Schema2EnvelopeContract(string messageId, Schema2AttributeContract[] envelopeAttributes, Schema2ItemContract[] items)
		{
			MessageId = messageId;
			EnvelopeAttributes = envelopeAttributes;
			Items = items;
		}
	}
}