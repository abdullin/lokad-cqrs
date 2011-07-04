#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    /// <summary>
    /// <para>Persists records in a tape stream, using SHA1 hashing and "magic" number sequences
    /// to detect corruption and offer partial recovery.</para>
    /// <para>System information is written in such a way, that if data is unicode human-readable, 
    /// then the file will be human-readable as well.</para>
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class FileTapeStream : ITapeStream
    {
        readonly FileInfo _data;
        readonly SHA1Managed _managed = new SHA1Managed();
        

        public FileTapeStream(string name)
        {
            _data = new FileInfo(name);
        }

        public IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("Offset can't be negative.", "offset");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Count must be greater than zero.", "maxCount");

            // index + maxCount - 1 > long.MaxValue, but transformed to avoid overflow
            if (offset > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");

            if (!_data.Exists)
                yield break;

            using (var file = _data.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                for (int i = 0; i < offset; i++)
                {
                    if (file.Position == file.Length)
                        yield break;

                    file.Seek(ReadableHeaderStart.Length, SeekOrigin.Current);
                    var dataLength = ReadReadableInt64(file);
                    var skip = dataLength + 16 + 16 + 28 + ReadableFooterEnd.Length + ReadableHeaderEnd.Length + ReadableFooterStart.Length;
                    file.Seek(skip, SeekOrigin.Current);
                }
                for (int i = 0; i < maxCount; i++)
                {
                    if (file.Position == file.Length)
                        yield break;
                    ReadAndVerifySignature(file, ReadableHeaderStart, "Start");
                    
                    var dataLength = ReadReadableInt64(file);

                    ReadAndVerifySignature(file, ReadableHeaderEnd, "Header-End");
                    var data = new byte[dataLength];
                    file.Read(data, 0, (int)dataLength);
                    ReadAndVerifySignature(file, ReadableFooterStart, "Footer-Start");

                    ReadReadableInt64(file);//length verified
                    var version = ReadReadableInt64(file);//version
                    var hash = ReadReadableHash(file);
                    var computed = _managed.ComputeHash(data);
                    if (!VerifyHash(computed, hash))
                    {
                        throw new InvalidOperationException("Hash corrupted");
                    }
                    ReadAndVerifySignature(file, ReadableFooterEnd, "End");
                    yield return new TapeRecord(version-1, data);
                }
                
            }
        }

        static readonly byte[] ReadableHeaderStart = Encoding.UTF8.GetBytes("/* header ");
        static readonly byte[] ReadableHeaderEnd = Encoding.UTF8.GetBytes(" */\r\n");

        static readonly byte[] ReadableFooterStart = Encoding.UTF8.GetBytes("\r\n/* footer ");
        static readonly byte[] ReadableFooterEnd = Encoding.UTF8.GetBytes(" */\r\n");
        

        public long GetCurrentVersion()
        {
            try
            {
                using (var s = _data.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    // seek end
                    s.Seek(0, SeekOrigin.End);
                    return ReadVersionFromTheEnd(s);
                }
            }
            catch (FileNotFoundException)
            {
                return 0;
            }
            catch (DirectoryNotFoundException)
            {
                return 0;
            }
        }
        
        public bool TryAppend(byte[] data, TapeAppendCondition condition)
        {
            if (data == null)
                throw new ArgumentNullException("records");

            if (data.Length == 0)
                throw new ArgumentException("Record must contain at least one byte.");
            using (var file = _data.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
            {
                file.Seek(0, SeekOrigin.End);
                // we need to know version first.
                var version = ReadVersionFromTheEnd(file);
                if (!condition.Satisfy(version))
                {
                    return false;
                }
                using (var writer = new BinaryWriter(file))
                {
                    writer.Write(ReadableHeaderStart);
                    WriteReadableInt64(file, data.Length);
                    writer.Write(ReadableHeaderEnd);
                    
                    writer.Write(data);
                    writer.Write(ReadableFooterStart);
                    WriteReadableInt64(file, data.Length);
                    WriteReadableInt64(file, version+1);
                    WriteReadableHash(file, _managed.ComputeHash(data));
                    writer.Write(ReadableFooterEnd);
                }
                return true;
            }
        }

        static long ReadVersionFromTheEnd(Stream file)
        {
            long version;
            if (file.Position == 0)
            {
                version = 0;
            }
            else
            {
                int seekBack = ReadableFooterEnd.Length + 28 + 16;
                file.Seek(-seekBack, SeekOrigin.Current);
                version = ReadReadableInt64(file);
                file.Seek(28, SeekOrigin.Current);
                ReadAndVerifySignature(file, ReadableFooterEnd, "End");
            }
            return version;
        }
        

        
        static void WriteReadableInt64(Stream stream, long value)
        {
            // long is 8 bytes ==> 16 bytes or readable unicode.
            var buffer = Encoding.UTF8.GetBytes(value.ToString("x16"));
            stream.Write(buffer, 0, 16);
        }

        static long ReadReadableInt64(Stream stream)
        {
            var buffer = new byte[16];
            stream.Read(buffer, 0, 16);
            var s = Encoding.UTF8.GetString(buffer);
            return long.Parse(s, NumberStyles.HexNumber);
        }
        static void WriteReadableHash(Stream stream, byte[] hash)
        {
            // hash is 20 bytes, which is encoded into 28 bytes of readable unicode
            var buffer = Encoding.UTF8.GetBytes(Convert.ToBase64String(hash));
            stream.Write(buffer,0,28);
        }
        static byte[] ReadReadableHash(Stream stream)
        {
            var buffer = new byte[28];
            stream.Read(buffer, 0, buffer.Length);
            var hash = Convert.FromBase64String(Encoding.UTF8.GetString(buffer));
            return hash;
        }

        static void ReadAndVerifySignature(Stream source, byte[] target, string name)
        {
            for (int i = 0; i < target.Length; i++)
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

        static bool VerifyHash(byte[] source, byte[] expected)
        {
            if (source.Length != expected.Length)
                return false;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != expected[i])
                    return false;
            }
            return true;

        }
    }
}
