namespace Lokad.Cqrs
{
	public sealed class Schema2ItemContract
	{
		public readonly string ContractName;
		public readonly int ContentSize;
		public Schema2AttributeContract[] Attributes;

		Schema2ItemContract()
		{
			Attributes = Empty;
		}

		public Schema2ItemContract(string contractName, int contentSize, Schema2AttributeContract[] attributes)
		{
			ContractName = contractName;
			ContentSize = contentSize;
			Attributes = attributes;
		}

		static readonly Schema2AttributeContract[] Empty = new Schema2AttributeContract[0];
	}
}