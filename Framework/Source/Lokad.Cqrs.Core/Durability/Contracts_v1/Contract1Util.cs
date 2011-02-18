using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Lmf
{
	static class Contract1Util
	{
		static AttributesContract ReadAttributes(byte[] message, MessageHeader header)
		{
			using (var stream = new MemoryStream(message, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				return Serializer.Deserialize<AttributesContract>(stream);
			}
		}

		public static string ReadReferenceMessage(byte[] buffer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.Contract1ReferenceFormat)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			return attributes.GetAttributeString(AttributeTypeContract.StorageReference)
				.ExposeException("Protocol violation: reference message should have storage reference");
		}

		public static byte[] SaveData(string contract, Guid messageId, Uri sender, IMessageSerializer serializer, object content)
		{
			var attribs = new List<AttributesItemContract>
				{
					new AttributesItemContract(AttributeTypeContract.ContractName, contract),
					new AttributesItemContract(AttributeTypeContract.Identity, messageId.ToString()),
					new AttributesItemContract(AttributeTypeContract.Sender, sender.ToString()),
					new AttributesItemContract(AttributeTypeContract.CreatedUtc, DateTime.UtcNow.ToBinary())
				};

			var attributes = new AttributesContract(attribs.ToArray());
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

		public static MessageEnvelope ReadDataMessage(byte[] buffer, IMessageSerializer serializer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.Contract1DataFormat)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			string contract = attributes
				.GetAttributeString(AttributeTypeContract.ContractName)
				.ExposeException("Protocol violation: message should have contract name");
			var type = serializer
				.GetTypeByContractName(contract)
				.ExposeException("Unsupported contract name: '{0}'", contract);
			string identity = attributes
				.GetAttributeString(AttributeTypeContract.Identity)
				.ExposeException("Protocol violation: message should have ID");

			var dict = new Dictionary<string, object>();
			foreach (var attribute in attributes.Items)
			{

				switch (attribute.Type)
				{
					case AttributeTypeContract.CreatedUtc:
						dict[EnvelopeAttribute.CreatedUtc] = DateTime.FromBinary(attribute.NumberValue);
						break;
					default:
						dict[attribute.GetName()] = attribute.GetValue();
						break;
				}
			}

			var index = MessageHeader.FixedSize + (int)header.AttributesLength;
			var count = (int)header.ContentLength;
			using (var stream = new MemoryStream(buffer, index, count))
			{
				var instance = serializer.Deserialize(stream, type);
				return new MessageEnvelope(identity, dict, instance, type);
			}
		}

		public static byte[] SaveReference(Uri storageContainer, string storageId, string contract, Guid messageId)
		{
			var attribs = new List<AttributesItemContract>
				{
					new AttributesItemContract(AttributeTypeContract.StorageContainer, storageContainer.ToString()),
					new AttributesItemContract(AttributeTypeContract.StorageReference, storageId),
					new AttributesItemContract(AttributeTypeContract.ContractName, contract),
					new AttributesItemContract(AttributeTypeContract.Identity, messageId.ToString())
				};
			
			var attributes = new AttributesContract(attribs.ToArray());

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
	}
}