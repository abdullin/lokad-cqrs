using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class SingleThreadFileTapeWriter : ISingleThreadTapeWriter
    {
        readonly string _dataFileName;
        readonly string _indexFileName;

        public SingleThreadFileTapeWriter(string name)
        {
            _dataFileName = Path.ChangeExtension(name, ".tmd");
            _indexFileName = Path.ChangeExtension(name, ".tmi");
        }

        /// <summary>
        /// For now it opens files for every call.
        /// </summary>
        /// <param name="records"></param>
        public void SaveRecords(IEnumerable<byte[]> records)
        {
            if (!records.Any())
                return;

            var writers = CreateWriters();

            try
            {
                var dataWriter = writers.DataWriter;
                var indexWriter = writers.IndexWriter;
                var dataStream = dataWriter.BaseStream;
                var indexStream = indexWriter.BaseStream;

                foreach (var record in records)
                {
                    if (record.Length == 0)
                        throw new ArgumentException("Record must contain at least one byte.");

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