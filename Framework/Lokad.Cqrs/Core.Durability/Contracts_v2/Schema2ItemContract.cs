using ProtoBuf;

namespace Lokad.Cqrs.Core.Durability.Contracts_v2
{
	[ProtoContract]
	public sealed class Schema2ItemContract
	{
		[ProtoMember(1)]
		public readonly string ContractName;
		[ProtoMember(2)]
		public readonly int ContentSize;
		[ProtoMember(3)]
		public Schema2ItemAttributeContract[] Attributes;

		Schema2ItemContract()
		{
			Attributes = Empty;
		}

		public Schema2ItemContract(string contractName, int contentSize, Schema2ItemAttributeContract[] attributes)
		{
			ContractName = contractName;
			ContentSize = contentSize;
			Attributes = attributes;
		}

		static readonly Schema2ItemAttributeContract[] Empty = new Schema2ItemAttributeContract[0];
	}
}