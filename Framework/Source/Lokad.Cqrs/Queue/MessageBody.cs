#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Lokad.Quality;
using ProtoBuf;

namespace Lokad.Cqrs.Queue
{
	public class UnpackedMessage
	{
		public readonly Type ContractType;
		public readonly MessageHeader Header;
		public readonly MessageAttribute[] Attributes;
		public readonly object Content;
		
		readonly IDictionary<string, object> _dynamicState = new Dictionary<string, object>();

		public UnpackedMessage(MessageHeader header, MessageAttribute[] attributes, object content, Type contractType)
		{
			Header = header;
			ContractType = contractType;
			Attributes = attributes;
			Content = content;
		}

		public Maybe<TValue> GetState<TValue>(string key)
		{
			return _dynamicState
				.GetValue(key)
				.Convert(o => (TValue) o);
		}

		public Maybe<TValue> GetState<TValue>()
		{
			return _dynamicState
				.GetValue(typeof(TValue).Name)
				.Convert(o => (TValue)o);
		}

		public TValue GetRequiredState<TValue>()
		{
			return GetState<TValue>().ExposeException("Should have required state " + typeof (TValue));
		}
		
		public UnpackedMessage WithState<TValue>(TValue value)
		{
			_dynamicState.Add(typeof(TValue).Name, value);
			return this;
		}

		public UnpackedMessage WithState<TValue>(string key, TValue value)
		{
			_dynamicState.Add(key, value);
			return this;
		}
	}

	public static class MessageUtil
	{
		public static MemoryStream SaveReferenceToStream(MessageAttributes attributes)
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

		public static MemoryStream SaveDataToStream(MessageAttributes messageAttributes, Action<Stream> message)
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
	}

	[ProtoContract]
	public sealed class MessageHeader
	{
		public const int FixedSize = 28;
		public const int CommonMessageFormatVersion = 2010020701;
		public const int ReferenceMessageFormatVersion = 2010020702;

		[ProtoMember(1, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int MessageFormatVersion;
		[ProtoMember(2, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long AttributesLength;
		[ProtoMember(3, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly long ContentLength;
		[ProtoMember(4, DataFormat = DataFormat.FixedSize, IsRequired = true)] public readonly int Checksum;

		public long GetTotalLength()
		{
			return FixedSize + AttributesLength + ContentLength;
		}


		public MessageHeader(int messageFormatVersion, long attributesLength, long contentLength, int checksum)
		{
			MessageFormatVersion = messageFormatVersion;
			AttributesLength = attributesLength;
			ContentLength = contentLength;
			Checksum = checksum;
		}

		public static MessageHeader ForData(long attributesLength, long contentLength, int checksum)
		{
			return new MessageHeader(CommonMessageFormatVersion, attributesLength, contentLength, checksum);
		}

		public static MessageHeader ForReference(long attributesLength, int checksum)
		{
			return new MessageHeader(ReferenceMessageFormatVersion, attributesLength, 0, checksum);
		}

		[UsedImplicitly]
		MessageHeader()
		{
		}
	}
}