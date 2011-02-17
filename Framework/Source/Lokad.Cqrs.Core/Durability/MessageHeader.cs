
using ProtoBuf;

namespace Lokad.Cqrs
{
	[ProtoContract]
	public sealed class MessageHeader
	{
		public const int FixedSize = 28;
		public const int DataMessageFormatVersion = 2010020701;
		public const int ReferenceMessageFormatVersion = 2010020702;

		[ProtoMember(1, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int MessageFormatVersion;
		[ProtoMember(2, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long AttributesLength;
		[ProtoMember(3, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long ContentLength;
		[ProtoMember(4, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int Checksum;

		public long GetTotalLength()
		{
			return FixedSize + AttributesLength + ContentLength;
		}


		public MessageHeader(int messageFormatVersion, long attributesLength, long contentLength, int checksum)
		{
			MessageFormatVersion = messageFormatVersion;
			AttributesLength = attributesLength;
			ContentLength = contentLength;
			Checksum = checksum;
		}

		public static MessageHeader ForData(long attributesLength, long contentLength, int checksum)
		{
			return new MessageHeader(DataMessageFormatVersion, attributesLength, contentLength, checksum);
		}

		public static MessageHeader ForReference(long attributesLength, int checksum)
		{
			return new MessageHeader(ReferenceMessageFormatVersion, attributesLength, 0, checksum);
		}

		
		MessageHeader()
		{
		}
	}
}