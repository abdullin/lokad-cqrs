using Lokad.Cqrs.Evil;
using ProtoBuf;

namespace Lokad.Cqrs.Durability.Contracts_v1
{
	[ProtoContract]
	public sealed class Schema1AttributesContract
	{
		[ProtoMember(1, DataFormat = DataFormat.Default)]
		public readonly Schema1AttributesItemContract[] Items;

		public Schema1AttributesContract(Schema1AttributesItemContract[] items)
		{
			Items = items;
		}
		// ReSharper disable UnusedMember.Local
		Schema1AttributesContract()
		{
			Items = new Schema1AttributesItemContract[0];
		}

		public Maybe<string> GetAttributeString(Schema1AttributeTypeContract type)
		{
			for (int i = Items.Length - 1; i >= 0; i--)
			{
				var item = Items[i];
				if (item.Type == type)
				{
					var value = item.StringValue;
					if (value == null)
						throw Errors.InvalidOperation("String attribute can't be null");
					return value;
				}
			}
			return Maybe<string>.Empty;
		}
	}
}