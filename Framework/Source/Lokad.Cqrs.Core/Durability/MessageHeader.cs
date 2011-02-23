
using System;
using ProtoBuf;

namespace Lokad.Cqrs
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

	[ProtoContract]
	public sealed class MessageEnvelopeContract
	{
		[ProtoMember(1)]
		public readonly string MessageId;

		public readonly Schema2AttributeContract[] EnvelopeAttributes;

		public readonly MessageItemContract[] Items;

	}

	public sealed class MessageItemContract
	{
		public readonly string ContractName;
		public readonly int ContentSize;
		public Schema2AttributeContract[] Attributes;

		MessageItemContract()
		{
			Attributes = Empty;
		}

		public MessageItemContract(string contractName, long contentSize, Schema2AttributeContract[] attributes)
		{
			ContractName = contractName;
			ContentSize = contentSize;
			Attributes = attributes;
		}

		static readonly Schema2AttributeContract[] Empty = new Schema2AttributeContract[0];
	}

	[ProtoContract]
	public sealed class MessageReferenceContract
	{
		[ProtoMember(1)]
		public readonly string EnvelopeId;
		[ProtoMember(2)]
		public readonly string StorageContainer;
		[ProtoMember(3)]
		public readonly string StorageReference;

		public MessageReferenceContract(string envelopeId, string storageContainer, string storageReference)
		{
			EnvelopeId = envelopeId;
			StorageContainer = storageContainer;
			StorageReference = storageReference;
		}


		MessageReferenceContract()
		{
		}

	}

	public sealed class Schema2AttributeContract
	{
		[ProtoMember(1)]
		public Schema2AttributeType Type { get; set; }
		[ProtoMember(2)]
		public string CustomName { get; set; }



		[ProtoMember(3)]
		private string StringValue { get; set; }
		[ProtoMember(4)]
		private long NumberValue { get; set; }
		[ProtoMember(5)]
		private Decimal DecimalValue { get; set; }
		[ProtoMember(6)]
		private byte[] ByteValue { get; set; }
	}



	public enum Schema2AttributeType
	{
		CreatedUtc,
		CustomString,
		CustomNumber
	}
}