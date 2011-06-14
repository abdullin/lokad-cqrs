using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class SingleThreadBlobTapeWriter : ISingleThreadTapeWriter
    {
        readonly CloudBlobContainer _container;
        readonly string _dataBlobName;
        readonly string _indexBlobName;

        public SingleThreadBlobTapeWriter(CloudBlobContainer container, string name)
        {
            _container = container;
            _dataBlobName = name;
            _indexBlobName = name + "-idx";
        }

        public void SaveRecords(IEnumerable<byte[]> records)
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

            Writers writers;

            var dataStream = dataBlob.OpenWrite();
            writers.DataWriter = new BinaryWriter(dataStream);

            var indexStream = indexBlob.OpenWrite();
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
