using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Lmf
{
	public static class MessageUtil
	{
		public static byte[] SaveReferenceMessage(Guid messageId, string contract, Uri storageContainer, string storageId)
		{
			return Contract1Util.SaveReference(storageContainer, storageId, contract, messageId);
		}

		public static byte[] SaveDataMessage(Guid messageId, string contract, Uri sender, IMessageSerializer serializer, object  content)
		{
			return Contract1Util.SaveData(contract, messageId, sender, serializer, content);
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