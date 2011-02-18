using System;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Lmf
{
	public static class MessageUtil
	{
		public static byte[] SaveReferenceMessageToStream(Guid messageId, string contract, Uri storageContainer, string storageId)
		{
			var stream = new MemoryStream();
			// skip header
			stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);


			var builder = new MessageAttributeBuilder();
			builder.AddBlobReference(storageContainer, storageId);
			builder.AddContract(contract);
			builder.AddIdentity(messageId.ToString());
			var instance = builder.Build();


			// write reference
			Serializer.Serialize(stream, instance);
			var attributesLength = stream.Position - MessageHeader.FixedSize;
			// write header
			stream.Seek(0, SeekOrigin.Begin);
			Serializer.Serialize(stream, MessageHeader.ForReference(attributesLength, 0));
			return stream.ToArray();
		}

		public static MemoryStream SaveDataMessageToStream(MessageAttributesContract messageAttributes, Action<Stream> message)
		{
			var stream = new MemoryStream();

			// skip header
			stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

			// save attributes

			Serializer.Serialize(stream, messageAttributes);
			var attributesLength = stream.Position - MessageHeader.FixedSize;

			// save message
			message(stream);
			// calculate length
			var bodyLength = stream.Position - attributesLength - MessageHeader.FixedSize;
			// write the header
			stream.Seek(0, SeekOrigin.Begin);
			var messageHeader = MessageHeader.ForData(attributesLength, bodyLength, 0);
			Serializer.Serialize(stream, messageHeader);
			return stream;
		}

		public static MessageHeader ReadHeader(byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer, 0, MessageHeader.FixedSize))
			{
				return Serializer.Deserialize<MessageHeader>(stream);
			}
		}

		public static MessageEnvelope ReadMessage(byte[] buffer, IMessageSerializer serializer, Func<string,byte[]> loadPackage)
		{
			// unefficient reading for now, since protobuf-net does not support reading parts
			var header = ReadHeader(buffer);
			if (header.MessageFormatVersion == MessageHeader.DataMessageFormatVersion)
			{
				return Contract1Util.ReadDataMessage(buffer, serializer);
			}
			if (header.MessageFormatVersion == MessageHeader.ReferenceMessageFormatVersion)
			{
				var reference = Contract1Util.ReadReferenceMessage(buffer);

				var blob = loadPackage(reference);
				return ReadMessage(blob, serializer, loadPackage);
			}
			throw Errors.InvalidOperation("Unknown message format: {0}", header.MessageFormatVersion);
		}
	}
}