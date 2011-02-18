using System;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Lmf
{
	public static class MessageUtil
	{
		public static byte[] SaveReferenceMessage(Guid messageId, string contract, Uri storageContainer, string storageId)
		{
			var builder = new MessageAttributeBuilder();
			builder.AddBlobReference(storageContainer, storageId);
			builder.AddContract(contract);
			builder.AddIdentity(messageId.ToString());
			var attributes = builder.Build();

			using (var stream = new MemoryStream())
			{
				// skip header
				stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);
				// write reference
				Serializer.Serialize(stream, attributes);
				var attributesLength = stream.Position - MessageHeader.FixedSize;
				// write header
				stream.Seek(0, SeekOrigin.Begin);
				Serializer.Serialize(stream, MessageHeader.ForReference(attributesLength, 0));
				return stream.ToArray();
			}
		}

		public static byte[] SaveDataMessage(Guid messageId, string contract, Uri sender, IMessageSerializer serializer, object  content)
		{
			var builder = new MessageAttributeBuilder();


			builder.AddContract(contract);
			builder.AddSender(sender.ToString());
			builder.AddIdentity(messageId.ToString());
			builder.AddCreated(DateTime.UtcNow);

			var attributes = builder.Build();


			using (var stream = new MemoryStream())
			{
				// skip header
				stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

				// save attributes

				Serializer.Serialize(stream, attributes);
				var attributesLength = stream.Position - MessageHeader.FixedSize;
				// save message
				serializer.Serialize(content, stream);
				// calculate length
				var bodyLength = stream.Position - attributesLength - MessageHeader.FixedSize;
				// write the header
				stream.Seek(0, SeekOrigin.Begin);
				var messageHeader = MessageHeader.ForData(attributesLength, bodyLength, 0);
				Serializer.Serialize(stream, messageHeader);
				return stream.ToArray();
			}
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