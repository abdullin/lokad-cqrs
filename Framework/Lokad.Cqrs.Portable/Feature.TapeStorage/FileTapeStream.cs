using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class FileTapeStream : ITapeStream
    {
        readonly string _dataFileName;
        readonly string _indexFileName;

        public FileTapeStream(string name)
        {
            _dataFileName = Path.ChangeExtension(name, ".tmd");
            _indexFileName = Path.ChangeExtension(name, ".tmi");
        }

        public IEnumerable<TapeRecord> ReadRecords(long version, int maxCount)
        {
            if (version< 0)
                throw new ArgumentOutOfRangeException("Offset can't be negative.", "offset");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Count must be greater than zero.", "maxCount");

            // index + maxCount - 1 > long.MaxValue, but transformed to avoid overflow
            if (version > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");

            Readers readers;
            if (!CheckGetReaders(out readers))
                yield break;

            try
            {
                var dataReader = readers.DataReader;
                var indexReader = readers.IndexReader;
                var dataStream = dataReader.BaseStream;
                var indexStream = indexReader.BaseStream;

                var indexOffset = (version) * sizeof(long);
                if (indexOffset >= indexStream.Length)
                    yield break;

                indexStream.Seek(indexOffset, SeekOrigin.Begin);
                dataStream.Seek(indexReader.ReadInt64(), SeekOrigin.Begin);

                var count = 0;
                var recordIndex = version;
                while (count < maxCount)
                {
                    if (dataStream.Position == dataStream.Length)
                        yield break;

                    var recordSize = dataReader.ReadInt32();
                    var data = dataReader.ReadBytes(recordSize);
                    yield return new TapeRecord(recordIndex, data);

                    count++;
                    recordIndex++;
                }
            }
            finally
            {
                DisposeReaders(readers);
            }
        }

        public long GetCurrentVersion()
        {
            Readers readers;
            if (!CheckGetReaders(out readers))
                return 0;

            try
            {
                return readers.IndexReader.BaseStream.Length / sizeof(long);
            }
            finally
            {
                DisposeReaders(readers);
            }
        }

        bool CheckGetReaders(out Readers readers)
        {
            var dataExists = File.Exists(_dataFileName);
            var indexExists = File.Exists(_indexFileName);

            // we return empty result if writer didn't even start writing to the storage.
            if (!(dataExists && indexExists))
            {
                readers = default(Readers); 
                return false;
            }

            if (!dataExists || !indexExists)
                throw new InvalidOperationException("Data and index file should exist both. Probable corruption.");

            readers = CreateReaders();
            return true;
        }

        

        Readers CreateReaders()
        {
            Readers readers;

            var data = new FileStream(_dataFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            readers.DataReader= new BinaryReader(data);

            var index = new FileStream(_indexFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            readers.IndexReader = new BinaryReader(index);

            return readers;
        }

        static void DisposeReaders(Readers readers)
        {
            readers.DataReader.Dispose(); // will dispose BaseStream too
            readers.IndexReader.Dispose(); // will dispose BaseStream too
        }

        struct Readers
        {
            internal BinaryReader DataReader;
            internal BinaryReader IndexReader;
        }

        /// <summary>
        /// For now it opens files for every call.
        /// </summary>
        /// <param name="records"></param>
        public void AppendRecords(ICollection<byte[]> records)
        {
            if (records == null)
                throw new ArgumentNullException("records");

            if (!records.Any())
                return;

            var writers = CreateWriters();

            try
            {
                var dataWriter = writers.DataWriter;
                var indexWriter = writers.IndexWriter;
                var dataStream = dataWriter.BaseStream;
                var indexStream = indexWriter.BaseStream;

                // Used only to enforce the rule that index must not be more than long.MaxValue
                var index = indexStream.Position / sizeof(long);

                foreach (var record in records)
                {
                    if (record.Length == 0)
                        throw new ArgumentException("Record must contain at least one byte.");

                    if (index > long.MaxValue - 1)
                        throw new IndexOutOfRangeException("Index is more than long.MaxValue.");
                    index++;

                    indexWriter.Write(dataStream.Position);

                    dataWriter.Write(record.Length);
                    dataWriter.Write(record);

                    dataStream.Flush();
                    indexStream.Flush();
                }
            }
            finally
            {
                DisposeWriters(writers);
            }
        }

        Writers CreateWriters()
        {
            var dataExists = File.Exists(_dataFileName);
            var indexExists = File.Exists(_indexFileName);

            if ((dataExists || indexExists) && (!dataExists || !indexExists))
            {
                if (!dataExists)
                    throw new InvalidOperationException("Index file found but no data file.");

                throw new InvalidOperationException("Data file found but no index file.");
            }

            Writers writers;

            var dataStream = new FileStream(_dataFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            writers.DataWriter = new BinaryWriter(dataStream);

            var indexStream = new FileStream(_indexFileName, FileMode.Append, FileAccess.Write, FileShare.Read);
            writers.IndexWriter = new BinaryWriter(indexStream);

            return writers;
        }

        static void DisposeWriters(Writers writers)
        {
            writers.DataWriter.Dispose(); // will dispose BaseStream too
            writers.IndexWriter.Dispose(); // will dispose BaseStream too
        }

        struct Writers
        {
            internal BinaryWriter DataWriter;
            internal BinaryWriter IndexWriter;
        }
    }
}