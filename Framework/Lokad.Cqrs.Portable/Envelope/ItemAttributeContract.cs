using System.Runtime.Serialization;

namespace Lokad.Cqrs.Envelope
{
	[DataContract]
	public sealed class ItemAttributeContract
	{
		[DataMember(Order = 1)]
		public ItemAttributeTypeContract Type { get; set; }
		[DataMember(Order = 2)]
		public string CustomName { get; set; }
		[DataMember(Order = 3)]
		public string StringValue { get; set; }
		[DataMember(Order = 4)]
		public long NumberValue { get; set; }
	}
}