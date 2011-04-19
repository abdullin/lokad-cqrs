using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport
{
	[ProtoContract]
	public sealed class MessageHeader
	{
		public const int FixedSize = 28;
		
		public const int Schema2DataFormat = 2011021801;

		[ProtoMember(1, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int MessageFormatVersion;
		[ProtoMember(2, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long AttributesLength;
		[ProtoMember(3, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long ContentLength;
		[ProtoMember(4, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int Checksum;

		public long TotalLength { get { return FixedSize + AttributesLength + ContentLength; } }

		MessageHeader(int messageFormatVersion, long attributesLength, long contentLength, int checksum)
		{
			MessageFormatVersion = messageFormatVersion;
			AttributesLength = attributesLength;
			ContentLength = contentLength;
			Checksum = checksum;
		}

		public static MessageHeader ForSchema2Data(long attributesLength, long contentLength)
		{
			return new MessageHeader(Schema2DataFormat, attributesLength, contentLength, 0);
		}

// ReSharper disable UnusedMember.Local
		MessageHeader()
// ReSharper restore UnusedMember.Local
		{
		}
	}
}