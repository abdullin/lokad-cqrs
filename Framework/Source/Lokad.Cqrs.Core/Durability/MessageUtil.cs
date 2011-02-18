#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using ProtoBuf;

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

		public static byte[] SaveReferenceMessage(Guid messageId, string contract, Uri storageContainer, string storageId)
		{
			return Contract1Util.SaveReference(storageContainer, storageId, contract, messageId);
		}

		public static byte[] SaveDataMessage(Guid messageId, string contract, Uri sender, IMessageSerializer serializer,
			object content)
		{
			return Contract1Util.SaveData(contract, messageId, sender, serializer, content);
		}

		public static MessageEnvelope ReadMessage(byte[] buffer, IMessageSerializer serializer,
			Func<string, byte[]> loadPackage)
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
				default:
					throw Errors.InvalidOperation("Unknown message format: {0}", header.MessageFormatVersion);
			}
		}
	}
}