using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SqlTapeStorageFactory : ITapeStorageFactory
    {
        readonly string _sqlConnectionString;
        readonly string _tableName;
        readonly ConcurrentDictionary<string, ITapeStream> _writers =
            new ConcurrentDictionary<string, ITapeStream>();

        public const string TableSchema = "dbo";

        public SqlTapeStorageFactory(string sqlConnectionString, string tableName)
        {
            _sqlConnectionString = sqlConnectionString;
            _tableName = tableName;
        }

        public ITapeStream GetOrCreateStream(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (String.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var writer = _writers.GetOrAdd(
                name,
                n => new SqlTapeStream(_sqlConnectionString, _tableName, name));

            return writer;
        }


        public void InitializeForWriting()
        {
            using (var conn = new SqlConnection(_sqlConnectionString))
            {
                conn.Open();

                EnsureTableExists(conn, _tableName);
            }
        }


        static void EnsureTableExists(SqlConnection conn, string tableName)
        {
            const string cmdText = @"
IF (NOT EXISTS (
    SELECT *
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_SCHEMA = @Schema AND TABLE_CATALOG = @Catalog AND TABLE_NAME = @Name))
BEGIN
    CREATE TABLE [{0}].[{1}] (
        [Stream] [nvarchar] (512) NOT NULL,
        [Version] [bigint] NOT NULL,
        [Data] [varbinary] (max) NOT NULL
        PRIMARY KEY CLUSTERED ([Stream], [Version])
    )
END";
            using (var cmd = new SqlCommand(String.Format(cmdText, TableSchema, tableName), conn))
            {
                cmd.Parameters.AddWithValue("@Schema", TableSchema);
                cmd.Parameters.AddWithValue("@Catalog", conn.Database);
                cmd.Parameters.AddWithValue("@Name", tableName);

                cmd.ExecuteNonQuery();
            }
        }
    }
}