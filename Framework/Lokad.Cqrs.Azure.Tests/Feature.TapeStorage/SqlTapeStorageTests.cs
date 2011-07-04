using System;
using System.Data.SqlClient;
using Lokad.Cqrs.Properties;
using NUnit.Framework;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    [TestFixture]
    public class SqlTapeStorageTests : TapeStorageTests
    {
        ITapeStorageFactory _storageFactory;

        protected override void PrepareEnvironment()
        {
            var connectionString = Settings.Default.SqlConnectionString;

            DatabaseHelper.CreateDatabase(connectionString);
        }

        protected override ITapeStream GetTapeStorageInterfaces()
        {
            var connectionString = Settings.Default.SqlConnectionString;
            var tableName = Settings.Default.SqlTapeWriterTableName;

            _storageFactory = new SqlTapeStorageFactory(connectionString, tableName);

            var count = 0;
            while (true)
            {
                count++;

                try
                {
                    _storageFactory.InitializeForWriting();
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

            

            const string name = "test";
            return _storageFactory.GetOrCreateStream(name);
        }

        protected override void FreeResources()
        {
            _storageFactory = null;
        }

        protected override void CleanupEnvironment()
        {
            var connectionString = Settings.Default.SqlConnectionString;

            try
            {
                DatabaseHelper.DeleteDatabase(connectionString);
            }
            catch (SqlException) {}
        }
    }
}
