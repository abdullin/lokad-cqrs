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

            var readAheadInBytes = _container.ServiceClient.ReadAheadInBytes;
            _container.ServiceClient.ReadAheadInBytes = 0;
            try
            {
                var dataReader = readers.DataReader;

                var range = GetReadRange(readers, index - 1, maxCount);
                var dataOffset = range.Item1;
                var dataSize = range.Item2;
                var recordCount = range.Item3;

                dataReader.BaseStream.Seek(dataOffset, SeekOrigin.Begin);
                var recordsBuffer = dataReader.ReadBytes(dataSize);

                using (var br = new BinaryReader(new MemoryStream(recordsBuffer)))
                {
                    var recordIndex = index;
                    var counter = 0;

                    var records = new List<TapeRecord>();

                    while (counter < recordCount)
                    {
                        var recordSize = br.ReadInt32();
                        var data = br.ReadBytes(recordSize);

                        records.Add(new TapeRecord(recordIndex, data));

                        counter++;
                        recordIndex++;
                    }

                    return records;
                }
            }
            finally
            {
                _container.ServiceClient.ReadAheadInBytes = readAheadInBytes;
                DisposeReaders(readers);
            }
        }

        static Tuple<long,int,int> GetReadRange(Readers readers, long firstIndex, int maxCount)
        {
            const int indexRecordSize = sizeof(long);

            var indexLength = readers.IndexReader.BaseStream.Length; // cache value to avoid many HTTP requests
            var indexCount = indexLength / indexRecordSize;

            if (firstIndex >= indexCount)
                return Tuple.Create(0L, 0, 0);

            var lastIndex = Math.Min(firstIndex + maxCount, indexCount);

            var readLastOffsetFromData = false;
            if (lastIndex == indexCount)
            {
                lastIndex--;
                readLastOffsetFromData = true;
            }

            var bytesToReadFromIndex = (lastIndex - firstIndex + 1) * indexRecordSize;
            if (bytesToReadFromIndex > int.MaxValue)
                throw new NotSupportedException("Can not read more than int.MaxValue records.");

            readers.IndexReader.BaseStream.Seek(firstIndex * indexRecordSize, SeekOrigin.Begin);

            long firstOffset;
            long lastOffset;

            // Read first and last index in one request
            // It's assumed that records will be read in small chunks
            var indexes = readers.IndexReader.ReadBytes((int) bytesToReadFromIndex);
            using (var br = new BinaryReader(new MemoryStream(indexes)))
            {
                firstOffset = br.ReadInt64();

                if (lastIndex == firstIndex)
                    lastOffset = firstOffset;
                else
                {
                    br.BaseStream.Seek(-indexRecordSize, SeekOrigin.End);
                    lastOffset = br.ReadInt64();
                }
            }

            var recordCount = (int) (lastIndex - firstIndex);
            long count;
            if (!readLastOffsetFromData)
            {
                count = lastOffset - firstOffset;
                if (count > int.MaxValue)
                    throw new NotSupportedException("Can not read more than int.MaxValue bytes of data.");

                return Tuple.Create(firstOffset, (int) count, recordCount);
            }

            readers.DataReader.BaseStream.Seek(lastOffset, SeekOrigin.Begin);
            var recordSize = readers.DataReader.ReadInt16();

            count = lastOffset + sizeof(int) + recordSize - firstOffset;
            if (count > int.MaxValue)
                throw new NotSupportedException("Can not read more than int.MaxValue bytes of data.");

            return Tuple.Create(firstOffset, (int) count, recordCount + 1);
        }

        Readers CreateReaders()
        {
            var dataBlob = _container.GetPageBlobReference(_dataBlobName);
            var indexBlob = _container.GetPageBlobReference(_indexBlobName);

            var dataExists = dataBlob.Exists();
            var indexExists = indexBlob.Exists();

            if (dataExists && !indexExists)
                throw new InvalidOperationException("Data blob found but no index blob.");
            if (!dataExists && indexExists)
                throw new InvalidOperationException("Index blob found but no data blob.");
            if (!dataExists && !indexExists)
                throw new InvalidOperationException("Neither data nor index blob found.");

            Readers readers;

            var dataStream = dataBlob.OpenReadAppending();
            readers.DataReader = new BinaryReader(dataStream);

            var indexStream = indexBlob.OpenReadAppending();
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
