using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public static class BlobRecordUtil
    {
        static readonly byte[] ReadableHeaderStart = Encoding.UTF8.GetBytes("/* header ");
        static readonly byte[] ReadableHeaderEnd = Encoding.UTF8.GetBytes(" */\r\n");
        static readonly byte[] ReadableFooterStart = Encoding.UTF8.GetBytes("\r\n/* footer ");
        static readonly byte[] ReadableFooterEnd = Encoding.UTF8.GetBytes(" */\r\n");
        static readonly byte[] Comma = Encoding.UTF8.GetBytes(",");
        static readonly SHA1Managed Sha1 = new SHA1Managed();

        public static int ReadRecordSize(this BinaryReader reader)
        {
            var stream = reader.BaseStream;

            stream.Seek(ReadableHeaderStart.Length, SeekOrigin.Current);

            var dataSize = reader.BaseStream.ReadReadableInt32(); // Length

            return
                ReadableHeaderStart.Length + 8 + ReadableHeaderEnd.Length +
                    dataSize +
                ReadableFooterStart.Length +
                    8 + // Length
                    Comma.Length +
                    16 + // Version
                    Comma.Length +
                    28 + // SHA1
                ReadableFooterEnd.Length;
        }

        public static TapeRecord ReadRecord(this BinaryReader reader)
        {
            var stream = reader.BaseStream;

            stream.ReadAndVerifySignature(ReadableHeaderStart, "Start");
            var recordSize = stream.ReadReadableInt32(); // Length
            stream.ReadAndVerifySignature(ReadableHeaderEnd, "Header-End");

            var data = reader.ReadBytes(recordSize);

            stream.ReadAndVerifySignature(ReadableFooterStart, "Footer-Start");
            stream.ReadReadableInt32(); // Length
            reader.ReadBytes(Comma.Length); // Comma
            var recVersion = stream.ReadReadableInt64(); // Version
            reader.ReadBytes(Comma.Length); // Comma
            var hash = stream.ReadReadableHash(); // SHA1 hash
            stream.ReadAndVerifySignature(ReadableFooterEnd, "End");

            var computed = Sha1.ComputeHash(data);
            if (!computed.SequenceEqual(hash))
                throw new InvalidOperationException("Hash corrupted");

            return new TapeRecord(recVersion, data);
        }

        public static void WriteRecord(this BinaryWriter writer, byte[] buffer, long version)
        {
            var stream = writer.BaseStream;

            writer.Write(ReadableHeaderStart);
            stream.WriteReadable(buffer.Length);
            writer.Write(ReadableHeaderEnd);

            writer.Write(buffer);

            writer.Write(ReadableFooterStart);
            stream.WriteReadable(buffer.Length);
            writer.Write(Comma);
            stream.WriteReadable(version);
            writer.Write(Comma);
            stream.WriteReadableHash(Sha1.ComputeHash(buffer));
            writer.Write(ReadableFooterEnd);
        }

        static void WriteReadable(this Stream stream, Int32 value)
        {
            // int is 4 bytes ==> 8 bytes of readable unicode.
            var buffer = Encoding.UTF8.GetBytes(value.ToString("x8"));
            stream.Write(buffer, 0, 8);
        }

        static int ReadReadableInt32(this Stream stream)
        {
            var buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            var s = Encoding.UTF8.GetString(buffer);
            return int.Parse(s, NumberStyles.HexNumber);
        }

        static void WriteReadable(this Stream stream, Int64 value)
        {
            // long is 8 bytes ==> 16 bytes of readable unicode.
            var buffer = Encoding.UTF8.GetBytes(value.ToString("x16"));
            stream.Write(buffer, 0, 16);
        }

        static long ReadReadableInt64(this Stream stream)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            var s = Encoding.UTF8.GetString(buffer);
            return long.Parse(s, NumberStyles.HexNumber);
        }

        static void WriteReadableHash(this Stream stream, byte[] hash)
        {
            // hash is 20 bytes, which is encoded into 28 bytes of readable unicode
            var buffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(hash));
            stream.Write(buffer, 0, 28);
        }

        static IEnumerable<byte> ReadReadableHash(this Stream stream)
        {
            var buffer = new byte[28];
            stream.Read(buffer, 0, buffer.Length);
            var hash = Convert.FromBase64String(Encoding.UTF8.GetString(buffer));
            return hash;
        }

        static void ReadAndVerifySignature(this Stream source, byte[] target, string name)
        {
            for (var i = 0; i < target.Length; i++)
            {
                var readByte = source.ReadByte();
                if (readByte == -1)
                    throw new InvalidOperationException(string.Format("Expected byte[{0}] of signature '{1}', but found EOL", i, name));
                if (readByte != target[i])
                {
                    throw new InvalidOperationException("Signature failed: " + name);
                }
            }
        }
    }
}