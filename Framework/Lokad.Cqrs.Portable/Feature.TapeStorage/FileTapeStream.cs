#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
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

            byte[] hash = new byte[20];
            // non-optimized by indexes for now.
            using (var file = _data.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(file))
            {
                for (int i = 0; i < offset; i++)
                {
                    if (file.Position == file.Length)
                        yield break;

                    file.Seek(Start.Length, SeekOrigin.Current);
                    var dataLength = reader.ReadInt32();
                    var skip = dataLength + 4 + 8 + 20 + End.Length;
                    file.Seek(skip, SeekOrigin.Current);
                }
                for (int i = 0; i < maxCount; i++)
                {
                    if (file.Position == file.Length)
                        yield break;

                    VerifySignature(file, Start, "Start");
                    var dataLengt = reader.ReadInt32();
                    var data = new byte[dataLengt];
                    file.Read(data, 0, dataLengt);
                    reader.ReadInt32();//length
                    var version = reader.ReadInt64();//version
                    file.Read(hash, 0, hash.Length);
                    var computed = _managed.ComputeHash(data);
                    if (!VerifyHash(computed, hash))
                    {
                        throw new InvalidOperationException("Hash corrupted");
                    }
                    VerifySignature(file, End, "End");
                    yield return new TapeRecord(version-1, data);
                }
                
            }
        }

        static readonly byte[] Start = Encoding.UTF8.GetBytes("[start](4D5E3FC3-C1782BB1B0BB)\r\n");
        static readonly byte[] End = Encoding.UTF8.GetBytes("\r\n[end](748E-4456-B110)");

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
                    writer.Write(Start);
                    writer.Write(data.Length);
                    writer.Write(data);
                    writer.Write(data.Length);
                    writer.Write(version + 1);
                    writer.Write(_managed.ComputeHash(data));
                    writer.Write(End);
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
                int seekBack = End.Length + 20 + 8;
                file.Seek(-seekBack, SeekOrigin.Current);
                var versionBuffer = new byte[8];
                file.Read(versionBuffer, 0, 8);
                version = BitConverter.ToInt64(versionBuffer, 0);
                file.Seek(20, SeekOrigin.Current);
                VerifySignature(file, End, "End");
            }
            return version;
        }

        static void VerifySignature(Stream source, byte[] target, string name)
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