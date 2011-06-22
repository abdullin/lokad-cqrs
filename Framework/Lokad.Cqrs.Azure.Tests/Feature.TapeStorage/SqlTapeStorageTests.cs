using Lokad.Cqrs.Properties;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class SqlTapeStorageTests : TapeStorageTests
    {
        ISingleThreadTapeWriterFactory _writerFactory;
        ITapeReaderFactory _readerFactory;

        protected override void SetUp()
        {
            var connectionString = Settings.Default.SqlConnectionString;
            var tableName = Settings.Default.SqlTapeWriterTableName;

            DatabaseHelper.CreateDatabase(connectionString);

            _writerFactory = new SingleThreadSqlTapeWriterFactory(connectionString, tableName);
            _writerFactory.Initialize();

            _readerFactory = new SqlTapeReaderFactory(connectionString, tableName);
        }

        protected override void TearDown()
        {
            var connectionString = Settings.Default.SqlConnectionString;

            DatabaseHelper.DeleteDatabase(connectionString);
        }

        protected override TestConfiguration GetConfiguration()
        {
            return new TestConfiguration
            {
                Name = "test",
                WriterFactory = _writerFactory,
                ReaderFactory = _readerFactory
            };
        }
    }
}
