using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport.Contracts_v2
{
	[ProtoContract]
	public sealed class Schema2ItemAttributeContract
	{
		[ProtoMember(1)]
		public Schema2ItemAttributeTypeContract Type { get; set; }
		[ProtoMember(2)]
		public string CustomName { get; set; }
		[ProtoMember(3)]
		public string StringValue { get; set; }
		[ProtoMember(4)]
		public long NumberValue { get; set; }
	}

	[ProtoContract]
	public sealed class Schema2EnvelopeAttributeContract
	{
		[ProtoMember(1)]
		public Schema2EnvelopeAttributeTypeContract Type { get; set; }
		[ProtoMember(2)]
		public string CustomName { get; set; }
		[ProtoMember(3)]
		public string StringValue { get; set; }
		[ProtoMember(4)]
		public long NumberValue { get; set; }
	}

	public enum Schema2EnvelopeAttributeTypeContract
	{
		Undefined = 0,
		CreatedUtc = 1,
		Sender = 2,
		CustomNumber = 3,
		CustomString = 4,
		
	}
}