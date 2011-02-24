using System;
using ProtoBuf;

namespace Lokad.Cqrs
{
	public sealed class Schema2AttributeContract
	{
		[ProtoMember(1)]
		public Schema2AttributeType Type { get; set; }
		[ProtoMember(2)]
		public string CustomName { get; set; }



		[ProtoMember(3)]
		private string StringValue { get; set; }
		[ProtoMember(4)]
		private long NumberValue { get; set; }
		[ProtoMember(5)]
		private Decimal DecimalValue { get; set; }
		[ProtoMember(6)]
		private byte[] ByteValue { get; set; }
	}
}