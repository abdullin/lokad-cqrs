using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class BlobTapeReader : ITapeReader
    {
        readonly CloudBlobContainer _container;
        readonly string _dataBlobName;
        readonly string _indexBlobName;

        public BlobTapeReader(CloudBlobContainer container, string name)
        {
            _container = container;
            _dataBlobName = name;
            _indexBlobName = name + "-idx";
        }

        public IEnumerable<TapeRecord> ReadRecords(long index, int maxCount)
        {
            if (index <= 0)
                throw new ArgumentOutOfRangeException("Must be more than zero.", "index");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Must be more than zero.", "maxCount");

            // index + maxCount - 1 > long.MaxValue, but transformed to avoid overflow
            if (index - 1 > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");

            var readers = CreateReaders();

            try
            {
                var dataReader = readers.DataReader;
                var indexReader = readers.IndexReader;
                var dataStream = dataReader.BaseStream;
                var indexStream = indexReader.BaseStream;

                var indexOffset = (index - 1) * sizeof(long);
                if (indexOffset >= indexStream.Length)
                    yield break;

                indexStream.Position = indexOffset;
                dataStream.Position = indexReader.ReadInt64();

                var count = 0;
                var recordIndex = index;
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
            var dataBlob = _container.GetBlockBlobReference(_dataBlobName);
            var indexBlob = _container.GetBlockBlobReference(_indexBlobName);

            var dataExists = dataBlob.Exists();
            var indexExists = indexBlob.Exists();

            if ((dataExists || indexExists) && (!dataExists || !indexExists))
            {
                if (!dataExists)
                    throw new InvalidOperationException("Index blob found but no data blob.");

                throw new InvalidOperationException("Data blob found but no index blob.");
            }

            Readers readers;

            var dataStream = dataBlob.OpenRead();
            readers.DataReader = new BinaryReader(dataStream);

            var indexStream = indexBlob.OpenRead();
            readers.IndexReader = new BinaryReader(indexStream);

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
    }
}
