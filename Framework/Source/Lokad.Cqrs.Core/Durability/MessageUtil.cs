#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using System.Linq;

namespace Lokad.Cqrs.Lmf
{
	public static class MessageUtil
	{
		public static MessageHeader ReadHeader(byte[] buffer)
		{
			using (var stream = new MemoryStream(buffer, 0, MessageHeader.FixedSize))
			{
				return Serializer.Deserialize<MessageHeader>(stream);
			}
		}

		public static byte[] SaveReferenceMessage(MessageReference reference)
		{
			return Contract2Util.SaveReference(reference);
		}

		public static byte[] SaveDataMessage(MessageEnvelopeBuilder builder, IMessageSerializer serializer)
		{
			return Contract2Util.SaveData(builder, serializer);
		}

		public static MessageEnvelope ReadMessage(byte[] buffer, IMessageSerializer serializer,
			Func<MessageReference, byte[]> loadPackage)
		{
			// unefficient reading for now, since protobuf-net does not support reading parts
			var header = ReadHeader(buffer);
			switch (header.MessageFormatVersion)
			{
				case MessageHeader.Contract1DataFormat:
					return Contract1Util.ReadDataMessage(buffer, serializer);
				case MessageHeader.Contract1ReferenceFormat:
					var reference = Contract1Util.ReadReferenceMessage(buffer);
					var blob = loadPackage(reference);
					return ReadMessage(blob, serializer, loadPackage);
				case MessageHeader.Schema2ReferenceFormat:
					var s2Reference = Contract2Util.ReadReference(buffer);
					var s2Blob = loadPackage(s2Reference);
					return ReadMessage(s2Blob, serializer, loadPackage);
				case MessageHeader.Schema2DataFormat:
					return Contract2Util.ReadDataMessage(buffer, serializer);
				default:
					throw Errors.InvalidOperation("Unknown message format: {0}", header.MessageFormatVersion);
			}
		}


	}

	public static class Contract2Util
	{
		public static byte[] SaveReference(MessageReference reference)
		{
			var contract = new MessageReferenceContract(reference.EnvelopeId, reference.StorageContainer, reference.StorageReference);

			using (var stream = new MemoryStream())
			{
				// skip header
				stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);
				// write reference
				Serializer.Serialize(stream, contract);
				var attributesLength = stream.Position - MessageHeader.FixedSize;
				// write header
				stream.Seek(0, SeekOrigin.Begin);
				Serializer.Serialize(stream, MessageHeader.ForSchema2Reference(attributesLength, 0));
				return stream.ToArray();
			}
		}

		public static MessageReference ReadReference(byte[] buffer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.Schema2ReferenceFormat)
				throw new InvalidOperationException("Unexpected message format");

			using (var memory = new MemoryStream(buffer))
			{
				memory.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);
				var contract = Serializer.Deserialize<MessageReferenceContract>(memory);

				return new MessageReference(contract.EnvelopeId, contract.StorageContainer, contract.StorageReference);
			}
		}

		static IDictionary<string,object> AttributesFromContract(Schema2AttributeContract[] attributes)
		{
			var dict = new Dictionary<string, object>();

			foreach (var attribute in attributes)
			{
				switch (attribute.Type)
				{
					
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			return dict;
		}

		static Schema2AttributeContract[] AttributesToContract(IDictionary<string,object > attributes)
		{
			var contracts = new Schema2AttributeContract[attributes.Count];
			var pos = 0;

			foreach (var attrib in attributes)
			{
				contracts[pos] =new Schema2AttributeContract();
			}

			return contracts;
		}

		public static MessageEnvelope ReadDataMessage(byte[] buffer, IMessageSerializer serializer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.Schema2DataFormat)
				throw new InvalidOperationException("Unexpected message format");


			MessageEnvelopeContract envelope;
			using (var stream = new MemoryStream(buffer, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				envelope = Serializer.Deserialize<MessageEnvelopeContract>(stream);
			}
			var index = MessageHeader.FixedSize + (int)header.AttributesLength;
			var count = (int)header.ContentLength;

			var items = new MessageItem[envelope.Items.Length];

			for (int i = 0; i < items.Length; i++)
			{
				var itemContract = envelope.Items[i];
				var type = serializer.GetTypeByContractName(itemContract.ContractName);
				var attributes = AttributesFromContract(itemContract.Attributes);

				if (type.HasValue)
				{
					using (var stream = new MemoryStream(buffer, index, itemContract.ContentSize))
					{
						var instance = serializer.Deserialize(stream, type.Value);
						
						items[i] = new MessageItem(itemContract.ContractName, type.Value, instance, attributes);
					}
				}
				else
				{
					// we can't deserialize. Keep it as buffer
					var bufferInstance = new byte[itemContract.ContentSize];
					Buffer.BlockCopy(buffer, index, bufferInstance, 0, itemContract.ContentSize);
					items[i] = new MessageItem(itemContract.ContractName, null, bufferInstance, attributes);
				}

				index += itemContract.ContentSize;
			}

			var envelopeAttributes = AttributesFromContract(envelope.EnvelopeAttributes);
			return new MessageEnvelope(envelope.MessageId, envelopeAttributes, items);
		}

		public static byte[] SaveData(MessageEnvelopeBuilder envelope, IMessageSerializer serializer)
		{

			//  string contract, Guid messageId, Uri sender, 

			var itemContracts = new MessageItemContract[envelope.Items.Count];
			using (var content = new MemoryStream())
			{
				var position = 0;
				for (int i = 0; i < envelope.Items.Count; i++)
				{
					var item = envelope.Items[i];
					var name = serializer.GetContractNameByType(item.MappedType)
						.ExposeException("Failed to find contract name for {0}", item.MappedType);
					serializer.Serialize(item.Content, content);
					var size = (int) content.Position - position;
					var attribContracts = AttributesToContract(item.Attributes);
					itemContracts[i] = new MessageItemContract(name, size, attribContracts);

					position += size;
				}

				var envelopeAttribs = AttributesToContract(envelope.Attributes);
				var contract = new MessageEnvelopeContract(envelope.EnvelopeId.ToString(), envelopeAttribs, itemContracts);

				using (var stream = new MemoryStream())
				{
					// skip header
					stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);
					// save envelope attributes
					Serializer.Serialize(stream, contract);
					var attributesLength = stream.Position - MessageHeader.FixedSize;
					// copy data
					content.WriteTo(stream);
					// calculate length
					var bodyLength = stream.Position - attributesLength - MessageHeader.FixedSize;

					// write the header
					stream.Seek(0, SeekOrigin.Begin);
					var messageHeader = MessageHeader.ForData(attributesLength, bodyLength, 0);
					Serializer.Serialize(stream, messageHeader);
					return stream.ToArray();
				}
			}
		}
	}
}