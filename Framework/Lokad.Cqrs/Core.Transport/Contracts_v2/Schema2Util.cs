using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Transport.Contracts_v2
{
	public static class Schema2Util
	{
		public static byte[] SaveReference(MessageReference reference)
		{
			var contract = new Schema2ReferenceContract(reference.EnvelopeId, reference.StorageContainer, reference.StorageReference);

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
				var contract = Serializer.Deserialize<Schema2ReferenceContract>(memory);

				return new MessageReference(contract.EnvelopeId, contract.StorageContainer, contract.StorageReference);
			}
		}

		static IDictionary<string,object> AttributesFromContract(IEnumerable<Schema2EnvelopeAttributeContract> attributes)
		{
			var dict = new Dictionary<string, object>();

			foreach (var attribute in attributes)
			{
				switch (attribute.Type)
				{
					case Schema2EnvelopeAttributeTypeContract.CreatedUtc:
						dict[MessageAttributes.Envelope.CreatedUtc] = DateTimeOffset.Parse(attribute.StringValue);
						break;
					case Schema2EnvelopeAttributeTypeContract.Sender:
						dict[MessageAttributes.Envelope.Sender] = attribute.CustomName;
						break;
					case Schema2EnvelopeAttributeTypeContract.CustomString:
						dict[attribute.CustomName] = attribute.StringValue;
						break;
					case Schema2EnvelopeAttributeTypeContract.CustomNumber:
						dict[attribute.CustomName] = attribute.NumberValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			return dict;
		}

		static IDictionary<string,object> AttributesFromContract(IEnumerable<Schema2ItemAttributeContract> attributes)
		{
			var dict = new Dictionary<string, object>();

			foreach (var attribute in attributes)
			{
				switch (attribute.Type)
				{
					case Schema2ItemAttributeTypeContract.CustomString:
						dict[attribute.CustomName] = attribute.StringValue;
						break;
					case Schema2ItemAttributeTypeContract.CustomNumber:
						dict[attribute.CustomName] = attribute.NumberValue;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			return dict;
		}

		static Schema2ItemAttributeContract[] ItemAttributesToContract(ICollection<KeyValuePair<string, object>> attributes)
		{
			var contracts = new Schema2ItemAttributeContract[attributes.Count];
			var pos = 0;

			foreach (var attrib in attributes)
			{
				switch (attrib.Key)
				{
					default:
						contracts[pos] = new Schema2ItemAttributeContract();
						throw new NotImplementedException("serializing item attributes is not supported now");
				}

				pos += 1;
			}

			return contracts;
		}

		/// <summary>
		/// Envelopes the attributes to contract.
		/// </summary>
		/// <param name="attributes">The attributes.</param>
		/// <returns></returns>
		static Schema2EnvelopeAttributeContract[] EnvelopeAttributesToContract(ICollection<KeyValuePair<string, object>> attributes)
		{
			var contracts = new Schema2EnvelopeAttributeContract[attributes.Count];
			var pos = 0;

			foreach (var attrib in attributes)
			{
				switch (attrib.Key)
				{
					case MessageAttributes.Envelope.CreatedUtc:
						contracts[pos] = new Schema2EnvelopeAttributeContract
							{
								Type = Schema2EnvelopeAttributeTypeContract.CreatedUtc,
								StringValue = ((DateTimeOffset) attrib.Value).ToString("o")
							};
						break;
					case MessageAttributes.Envelope.Sender:
						contracts[pos] = new Schema2EnvelopeAttributeContract
							{
								Type = Schema2EnvelopeAttributeTypeContract.Sender,
								StringValue = (string) attrib.Value
							};
						break;
					default:
						if (attrib.Value is string)
						{
							contracts[pos] = new Schema2EnvelopeAttributeContract
								{
									Type = Schema2EnvelopeAttributeTypeContract.CustomString,
									CustomName = attrib.Key,
									StringValue = (string) attrib.Value
								};
						}
						else if ((attrib.Value is long) || (attrib.Value is int) || (attrib.Value is short))
						{
							contracts[pos] = new Schema2EnvelopeAttributeContract
							{
								Type = Schema2EnvelopeAttributeTypeContract.CustomNumber,
								CustomName = attrib.Key,
								NumberValue = Convert.ToInt64(attrib.Value)
							};
						}
						else
						{
							throw new NotSupportedException("serialization of generic attributes is not supported yet");
						}
						break;
				}
				pos += 1;


			}

			return contracts;
		}

		public static MessageEnvelope ReadDataMessage(byte[] buffer, IMessageSerializer serializer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.Schema2DataFormat)
				throw new InvalidOperationException("Unexpected message format");


			Schema2EnvelopeContract envelope;
			using (var stream = new MemoryStream(buffer, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				envelope = Serializer.Deserialize<Schema2EnvelopeContract>(stream);
			}
			var index = MessageHeader.FixedSize + (int)header.AttributesLength;
			//var count = (int)header.ContentLength;

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
						
						items[i] = new MessageItem(type.Value, instance, attributes);
					}
				}
				else
				{
					// we can't deserialize. Keep it as buffer
					var bufferInstance = new byte[itemContract.ContentSize];
					Buffer.BlockCopy(buffer, index, bufferInstance, 0, itemContract.ContentSize);
					items[i] = new MessageItem(null, bufferInstance, attributes);
				}

				index += itemContract.ContentSize;
			}

			var envelopeAttributes = AttributesFromContract(envelope.EnvelopeAttributes);
			return new MessageEnvelope(envelope.EnvelopeId, envelopeAttributes, items, envelope.DeliverOnUtc);
		}

		public static byte[] SaveData(MessageEnvelope envelope, IMessageSerializer serializer)
		{
			//  string contract, Guid messageId, Uri sender, 
			var itemContracts = new Schema2ItemContract[envelope.Items.Length];
			using (var content = new MemoryStream())
			{
				var position = 0;
				for (int i = 0; i < envelope.Items.Length; i++)
				{
					var item = envelope.Items[i];
					var name = serializer.GetContractNameByType(item.MappedType)
						.ExposeException("Failed to find contract name for {0}", item.MappedType);
					serializer.Serialize(item.Content, content);
					var size = (int) content.Position - position;
					var attribContracts = ItemAttributesToContract(item.GetAllAttributes());
					itemContracts[i] = new Schema2ItemContract(name, size, attribContracts);

					position += size;
				}

				var envelopeAttribs = EnvelopeAttributesToContract(envelope.GetAllAttributes());

				
				
				var contract = new Schema2EnvelopeContract(envelope.EnvelopeId.ToString(), envelopeAttribs, itemContracts, envelope.DeliverOn);

				using (var stream = new MemoryStream())
				{
					// skip header
					stream.Seek(MessageHeader.FixedSize, SeekOrigin.Begin);
					// save envelope attributes
					Serializer.Serialize(stream, contract);
					var attributesLength = stream.Position - MessageHeader.FixedSize;
					// copy data
					content.WriteTo(stream);
					// write the header
					stream.Seek(0, SeekOrigin.Begin);
					var messageHeader = MessageHeader.ForSchema2Data(attributesLength, content.Position);
					Serializer.Serialize(stream, messageHeader);
					return stream.ToArray();
				}
			}
		}
	}
}