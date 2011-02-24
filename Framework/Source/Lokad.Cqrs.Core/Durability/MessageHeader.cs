using ProtoBuf;

namespace Lokad.Cqrs.Durability
{
	[ProtoContract]
	public sealed class MessageHeader
	{
		public const int FixedSize = 28;
		public const int Contract1DataFormat = 2010020701;
		public const int Contract1ReferenceFormat = 2010020702;

		public const int Schema2ReferenceFormat = 2011021802;
		public const int Schema2DataFormat = 2011021801;

		[ProtoMember(1, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int MessageFormatVersion;
		[ProtoMember(2, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long AttributesLength;
		[ProtoMember(3, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long ContentLength;
		[ProtoMember(4, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int Checksum;

		//public long GetTotalLength()
		//{
		//    return FixedSize + AttributesLength + ContentLength;
		//}

		public long TotalLength { get { return FixedSize + AttributesLength + ContentLength; } }


		MessageHeader(int messageFormatVersion, long attributesLength, long contentLength, int checksum)
		{
			MessageFormatVersion = messageFormatVersion;
			AttributesLength = attributesLength;
			ContentLength = contentLength;
			Checksum = checksum;
		}

		public static MessageHeader ForData(long attributesLength, long contentLength, int checksum)
		{
			return new MessageHeader(Contract1DataFormat, attributesLength, contentLength, checksum);
		}

		public static MessageHeader ForSchema1Reference(long attributesLength, int checksum)
		{
			return new MessageHeader(Contract1ReferenceFormat, attributesLength, 0, checksum);
		}
		public static MessageHeader ForSchema2Reference(long attributesLength, int checksum)
		{
			return new MessageHeader(Schema2ReferenceFormat, attributesLength, 0, checksum);
		}
	}
}