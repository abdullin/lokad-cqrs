#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Envelope
{
	public sealed class MessageHeader
	{
		public const int FixedSize = 20;

		public const int Schema2DataFormat = 2011021801;

		public readonly int MessageFormatVersion;
		public readonly long EnvelopeBytes;
		public readonly long CheckSum;

		public MessageHeader(int messageFormatVersion, long envelopeBytes, long checksum)
		{
			MessageFormatVersion = messageFormatVersion;
			EnvelopeBytes = envelopeBytes;
			CheckSum = checksum;
		}

		public static MessageHeader ReadHeader(byte[] buffer)
		{
			var magic = BitConverter.ToInt32(buffer, 0);
			var envelopeBytes = BitConverter.ToInt64(buffer, 4);
			var checkSum = BitConverter.ToInt64(buffer, 4 + 8);

			return new MessageHeader(magic, envelopeBytes, checkSum);
		}

		public void WriteToStream(MemoryStream stream)
		{
			stream.Write(BitConverter.GetBytes(MessageFormatVersion), 0, 4);
			stream.Write(BitConverter.GetBytes(EnvelopeBytes), 0, 8);
			stream.Write(BitConverter.GetBytes(CheckSum), 0, 8);
		}
	}
}