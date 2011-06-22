using System;
using Lokad.Cqrs.Properties;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class SqlTapeStorageTests : TapeStorageTests
    {
        ISingleThreadTapeWriterFactory _writerFactory;
        ITapeReaderFactory _readerFactory;

        protected override void PrepareEnvironment()
        {
            var connectionString = Settings.Default.SqlConnectionString;

            DatabaseHelper.CreateDatabase(connectionString);
        }

        protected override Factories GetTapeStorageInterfaces()
        {
            var connectionString = Settings.Default.SqlConnectionString;
            var tableName = Settings.Default.SqlTapeWriterTableName;

            _writerFactory = new SingleThreadSqlTapeWriterFactory(connectionString, tableName);

            var count = 0;
            while (true)
            {
                count++;

                try
                {
                    _writerFactory.Initialize();
                    break;
                }
                catch (Exception)
                {
                    if (count < 1)
                    {
                        System.Threading.Thread.Sleep(500);
                        continue;
                    }

                    throw;
                }
            }

            _readerFactory = new SqlTapeReaderFactory(connectionString, tableName);

            const string name = "test";
            return new Factories
                {
                    Writer = _writerFactory.GetOrCreateWriter(name),
                    Reader = _readerFactory.GetReader(name)
                };
        }

        protected override void FreeResources()
        {
            _writerFactory = null;
            _readerFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            var connectionString = Settings.Default.SqlConnectionString;

            DatabaseHelper.DeleteDatabase(connectionString);
        }
    }
}
