#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
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

		public static byte[] SaveReferenceMessage(MessageReference reference)
		{
			return Contract2Util.SaveReference(reference);
		}

		public static byte[] SaveDataMessage(Guid messageId, string contract, Uri sender, IMessageSerializer serializer,
			object content)
		{
			return Contract1Util.SaveData(contract, messageId, sender, serializer, content);
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
	}
}