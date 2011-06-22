using System;
using System.Collections.Generic;
using System.IO;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class FileTapeReader : ITapeReader
    {
        readonly string _dataFileName;
        readonly string _indexFileName;

        public FileTapeReader(string name)
        {
            _dataFileName = Path.ChangeExtension(name, ".tmd");
            _indexFileName = Path.ChangeExtension(name, ".tmi");
        }

        public IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount)
        {
            if (offset< 0)
                throw new ArgumentOutOfRangeException("Offset can't be negative.", "offset");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Count must be greater than zero.", "maxCount");

            // index + maxCount - 1 > long.MaxValue, but transformed to avoid overflow
            if (offset > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");

            var readers = CreateReaders();

            try
            {
                var dataReader = readers.DataReader;
                var indexReader = readers.IndexReader;
                var dataStream = dataReader.BaseStream;
                var indexStream = indexReader.BaseStream;

                var indexOffset = (offset) * sizeof(long);
                if (indexOffset >= indexStream.Length)
                    yield break;

                indexStream.Seek(indexOffset, SeekOrigin.Begin);
                dataStream.Seek(indexReader.ReadInt64(), SeekOrigin.Begin);

                var count = 0;
                var recordIndex = offset;
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

        Readers CreateReaders()
        {
            var dataExists = File.Exists(_dataFileName);
            var indexExists = File.Exists(_indexFileName);

            if (!dataExists || !indexExists)
                throw new InvalidOperationException("Data or index file not found.");

            Readers readers;

            var data = new FileStream(_dataFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            readers.DataReader= new BinaryReader(data);

            var index = new FileStream(_indexFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            readers.IndexReader = new BinaryReader(index);

            return readers;
        }

        static void DisposeReaders(Readers writers)
        {
            writers.DataReader.Dispose(); // will dispose BaseStream too
            writers.IndexReader.Dispose(); // will dispose BaseStream too
        }

        struct Readers
        {
            internal BinaryReader DataReader;
            internal BinaryReader IndexReader;
        }
    }
}