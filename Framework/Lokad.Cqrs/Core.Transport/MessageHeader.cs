#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Core.Transport
{
	public sealed class MessageHeader
	{
		public const int FixedSize = 28;

		public const int Schema2DataFormat = 2011021801;

		public readonly int MessageFormatVersion;
		public readonly long ContentLength;
		public readonly long AttributesLength;
		public readonly long CheckSum;

		public MessageHeader(int messageFormatVersion, long attributesLength, long contentLength, long checksum)
		{
			MessageFormatVersion = messageFormatVersion;
			AttributesLength = attributesLength;
			ContentLength = contentLength;
			CheckSum = checksum;
		}

		public static MessageHeader ReadHeader(byte[] buffer, int start = 0)
		{
			var magic = BitConverter.ToInt32(buffer, start);
			var attributesLength = BitConverter.ToInt64(buffer, start + 4);
			var contentLength = BitConverter.ToInt32(buffer, start + 4 + 8);

			var checkSum = BitConverter.ToInt64(buffer, start + 4 + 16);

			return new MessageHeader(magic, attributesLength, contentLength, checkSum);
		}

		public void WriteToStream(MemoryStream stream)
		{
			stream.Write(BitConverter.GetBytes(MessageFormatVersion), 0, 4);
			stream.Write(BitConverter.GetBytes(AttributesLength), 0, 8);
			stream.Write(BitConverter.GetBytes(ContentLength), 0, 8);
			stream.Write(BitConverter.GetBytes(CheckSum), 0, 8);
		}
	}
}