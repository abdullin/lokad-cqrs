using System;
using System.Data.SqlClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class TestDatabase : IDisposable
    {
        readonly string _connectionString;

        public TestDatabase(string connectionString)
        {
            _connectionString = connectionString;

            CreateDatabase();
        }

        public void Dispose()
        {
            DeleteDatabase();
        }

        void CreateDatabase()
        {
            string databaseName;

            if (!GetDatabaseName(out databaseName))
                throw new InvalidOperationException("Can not get database name.");

            using (var conn = new SqlConnection(ConnectionToMaster(databaseName)))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "CREATE DATABASE " + databaseName;
                    command.ExecuteNonQuery();
                }
            }
        }

        bool GetDatabaseName(out string database)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                database = conn.Database;

                if (!string.IsNullOrWhiteSpace(database))
                    return true;

                return false;
            }
        }

        string ConnectionToMaster(string originalInitialCatalog)
        {
            return _connectionString.Replace("=" + originalInitialCatalog + ";", "=master;");
        }

        void DeleteDatabase()
        {
            string databaseName;

            if (!GetDatabaseName(out databaseName))
                throw new InvalidOperationException("Can not get database name.");

            using (var conn = new SqlConnection(ConnectionToMaster(databaseName)))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "DROP DATABASE " + databaseName;
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
