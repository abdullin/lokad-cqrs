using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Lmf
{
	public static class MessageUtil
	{
		public static MemoryStream SaveReferenceMessageToStream(MessageAttributesContract attributes)
		{
			var stream = new MemoryStream();
			// skip header
			stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);

			// write reference
			Serializer.Serialize(stream, attributes);
			long attributesLength = stream.Position - MessageHeader.FixedSize;
			// write header
			stream.Seek(0, SeekOrigin.Begin);
			Serializer.Serialize(stream, MessageHeader.ForReference(attributesLength, 0));
			return stream;
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
			var bodyLength = stream.Position - attributesLength - MessageHeader.FixedSize;
			// write the header
			stream.Seek(0, SeekOrigin.Begin);
			var messageHeader = MessageHeader.ForData(attributesLength, bodyLength, 0);
			Serializer.Serialize(stream, messageHeader);
			return stream;
		}

		public static MessageAttributesContract ReadAttributes(byte[] message, MessageHeader header)
		{
			using (var stream = new MemoryStream(message, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				return Serializer.Deserialize<MessageAttributesContract>(stream);
			}
		}

		public static MessageHeader ReadHeader(byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer, 0, MessageHeader.FixedSize))
			{
				return Serializer.Deserialize<MessageHeader>(stream);
			}
		}

		public static string ReadReferenceMessage(byte[] buffer)
		{
			var header = ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.ReferenceMessageFormatVersion)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			return attributes.GetAttributeString(MessageAttributeTypeContract.StorageReference)
				.ExposeException("Protocol violation: reference message should have storage reference");
		}

		public static MessageEnvelope ReadDataMessage(byte[] buffer, IMessageSerializer serializer)
		{
			var header = ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.DataMessageFormatVersion)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			string contract = attributes
				.GetAttributeString(MessageAttributeTypeContract.ContractName)
				.ExposeException("Protocol violation: message should have contract name");
			var type = serializer
				.GetTypeByContractName(contract)
				.ExposeException("Unsupported contract name: '{0}'", contract);

			var dict = new Dictionary<string, object>();
			foreach (var attribute in attributes.Items)
			{
				dict[attribute.GetName()] = attribute.GetValue();
			}

			var index = MessageHeader.FixedSize + (int)header.AttributesLength;
			var count = (int)header.ContentLength;
			using (var stream = new MemoryStream(buffer, index, count))
			{
				var instance = serializer.Deserialize(stream, type);
				return new MessageEnvelope(dict, instance, type);
			}
		}
	}
}