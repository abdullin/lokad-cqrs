using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Lmf
{
	static class Contract1Util
	{
		static MessageAttributesContract ReadAttributes(byte[] message, MessageHeader header)
		{
			using (var stream = new MemoryStream(message, MessageHeader.FixedSize, (int) header.AttributesLength))
			{
				return Serializer.Deserialize<MessageAttributesContract>(stream);
			}
		}

		public static string ReadReferenceMessage(byte[] buffer)
		{
			var header = MessageUtil.ReadHeader(buffer);
			if (header.MessageFormatVersion != MessageHeader.ReferenceMessageFormatVersion)
				throw new InvalidOperationException("Unexpected message format");

			var attributes = ReadAttributes(buffer, header);
			return attributes.GetAttributeString(MessageAttributeTypeContract.StorageReference)
				.ExposeException("Protocol violation: reference message should have storage reference");
		}

		public static MessageEnvelope ReadDataMessage(byte[] buffer, IMessageSerializer serializer)
		{
			var header = MessageUtil.ReadHeader(buffer);
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