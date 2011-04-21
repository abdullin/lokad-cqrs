using System.Runtime.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport.Contracts_v2
{
	[DataContract]
	public sealed class Schema2ItemContract
	{
		[DataMember(Order = 1)]
		public readonly string ContractName;
		[DataMember(Order = 2)]
		public readonly int ContentSize;
		[DataMember(Order = 3)]
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