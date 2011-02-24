#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Cqrs.Durability.Contracts_v1;
using Lokad.Cqrs.Durability.Contracts_v2;
using Lokad.Cqrs.Evil;
using ProtoBuf;

namespace Lokad.Cqrs.Durability
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
			return Schema2Util.SaveReference(reference);
		}

		public static byte[] SaveDataMessage(MessageEnvelopeBuilder builder, IMessageSerializer serializer)
		{
			return Schema2Util.SaveData(builder, serializer);
		}

		public static MessageEnvelope ReadMessage(byte[] buffer, IMessageSerializer serializer,
			Func<MessageReference, byte[]> loadPackage = null)
		{
			if (null == loadPackage)
			{
				loadPackage = reference => { throw Errors.InvalidOperation("Package loading not supported"); };
			}


			// unefficient reading for now, since protobuf-net does not support reading parts
			var header = ReadHeader(buffer);
			switch (header.MessageFormatVersion)
			{
				case MessageHeader.Contract1DataFormat:
					return Schema1Util.ReadDataMessage(buffer, serializer);
				case MessageHeader.Contract1ReferenceFormat:
					var reference = Schema1Util.ReadReferenceMessage(buffer);
					var blob = loadPackage(reference);
					return ReadMessage(blob, serializer, loadPackage);
				case MessageHeader.Schema2ReferenceFormat:
					var s2Reference = Schema2Util.ReadReference(buffer);
					var s2Blob = loadPackage(s2Reference);
					return ReadMessage(s2Blob, serializer, loadPackage);
				case MessageHeader.Schema2DataFormat:
					return Schema2Util.ReadDataMessage(buffer, serializer);
				default:
					throw Errors.InvalidOperation("Unknown message format: {0}", header.MessageFormatVersion);
			}
		}


	}
}