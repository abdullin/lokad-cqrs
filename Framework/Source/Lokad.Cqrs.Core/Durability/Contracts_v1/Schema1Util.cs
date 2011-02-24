using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Durability.Contracts_v1
{
	static class Schema1Util
	{
		static Schema1AttributesContract ReadAttributes(byte[] message, MessageHeader header)
		{
			using (var stream = new MemoryStream(message, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				return Serializer.Deserialize<Schema1AttributesContract>(stream);
			}
		}

		public static byte[] SaveReference(Uri storageContainer, string storageId, string contract, Guid messageId)
		{
			var attribs = new List<Schema1AttributesItemContract>
				{
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.StorageContainer, storageContainer.ToString()),
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.StorageReference, storageId),
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.ContractName, contract),
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.Identity, messageId.ToString())
				};
			
			var attributes = new Schema1AttributesContract(attribs.ToArray());

			using (var stream = new MemoryStream())
			{
				// skip header
				stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);
				// write reference
				Serializer.Serialize(stream, attributes);
				var attributesLength = stream.Position - MessageHeader.FixedSize;
				// write header
				stream.Seek(0, SeekOrigin.Begin);
				Serializer.Serialize(stream, MessageHeader.ForSchema1Reference(attributesLength, 0));
				return stream.ToArray();
			}
		}

		public static MessageReference ReadReferenceMessage(byte[] buffer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.Contract1ReferenceFormat)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			var refernce = attributes.GetAttributeString(Schema1AttributeTypeContract.StorageReference)
				.ExposeException("Protocol violation: reference message should have storage reference");

			var container = attributes.GetAttributeString(Schema1AttributeTypeContract.StorageContainer)
				.ExposeException("Protocol violation: reference message should have storage container");

			var identity = attributes.GetAttributeString(Schema1AttributeTypeContract.Identity)
				.ExposeException("Protocol violation: reference message should have storage container");
			return new MessageReference(identity, refernce, container);
		}

		public static byte[] SaveData(string contract, Guid messageId, Uri sender, IMessageSerializer serializer, object content)
		{
			var attribs = new List<Schema1AttributesItemContract>
				{
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.ContractName, contract),
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.Identity, messageId.ToString()),
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.Sender, sender.ToString()),
					new Schema1AttributesItemContract(Schema1AttributeTypeContract.CreatedUtc, DateTime.UtcNow.ToBinary())
				};

			var attributes = new Schema1AttributesContract(attribs.ToArray());
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
				.GetAttributeString(Schema1AttributeTypeContract.ContractName)
				.ExposeException("Protocol violation: message should have contract name");
			var type = serializer
				.GetTypeByContractName(contract)
				.ExposeException("Unsupported contract name: '{0}'", contract);
			string messageId = attributes
				.GetAttributeString(Schema1AttributeTypeContract.Identity)
				.ExposeException("Protocol violation: message should have ID");

			var envelope = new Dictionary<string, object>();
			var itemDict = new Dictionary<string, object>();
			foreach (var attribute in attributes.Items)
			{

				switch (attribute.Type)
				{
					case Schema1AttributeTypeContract.ContractName:
					case Schema1AttributeTypeContract.Identity:
						// skip these, they already are retrieved
						break;
					case Schema1AttributeTypeContract.CreatedUtc:
						envelope[MessageAttributes.Envelope.CreatedUtc] = DateTime.FromBinary(attribute.NumberValue);
						break;
					case Schema1AttributeTypeContract.Sender:
						envelope[MessageAttributes.Envelope.Sender] = attribute.StringValue;
						break;
					default:
						envelope[attribute.GetName()] = attribute.GetValue();
						itemDict[attribute.GetName()] = attribute.GetValue();
						break;
				}
			}

			var index = MessageHeader.FixedSize + (int)header.AttributesLength;
			var count = (int)header.ContentLength;
			using (var stream = new MemoryStream(buffer, index, count))
			{
				var instance = serializer.Deserialize(stream, type);
				var item = new MessageItem(contract, type, instance, envelope);
				return new MessageEnvelope(messageId, envelope, new[] {item});
			}
		}
	}
}