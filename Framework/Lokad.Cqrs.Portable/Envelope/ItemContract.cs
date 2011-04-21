using System.Runtime.Serialization;

namespace Lokad.Cqrs.Envelope
{
	[DataContract]
	public sealed class ItemContract
	{
		[DataMember(Order = 1)]
		public readonly string ContractName;
		[DataMember(Order = 2)]
		public readonly int ContentSize;
		[DataMember(Order = 3)]
		public ItemAttributeContract[] Attributes;

		ItemContract()
		{
			Attributes = Empty;
		}

		public ItemContract(string contractName, int contentSize, ItemAttributeContract[] attributes)
		{
			ContractName = contractName;
			ContentSize = contentSize;
			Attributes = attributes;
		}

		static readonly ItemAttributeContract[] Empty = new ItemAttributeContract[0];
	}
}