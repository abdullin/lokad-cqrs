using System.Runtime.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport.Contracts_v2
{
	[DataContract]
	public sealed class EnvelopeAttributeContract
	{
		[DataMember(Order = 1)]
		public EnvelopeAttributeTypeContract Type { get; set; }
		[DataMember(Order = 2)]
		public string CustomName { get; set; }
		[DataMember(Order = 3)]
		public string StringValue { get; set; }
		[DataMember(Order = 4)]
		public long NumberValue { get; set; }
	}
}