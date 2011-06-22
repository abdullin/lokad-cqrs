using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SingleThreadSqlTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        public const string TableSchema = "dbo";

        readonly string _sqlConnectionString;
        readonly string _tableName;

        readonly ConcurrentDictionary<string, ISingleThreadTapeWriter> _writers =
            new ConcurrentDictionary<string, ISingleThreadTapeWriter>();

        public SingleThreadSqlTapeWriterFactory(string sqlConnectionString, string tableName)
        {
            _sqlConnectionString = sqlConnectionString;
            _tableName = tableName;
        }

        public void Initialize()
        {
            using (var conn = new SqlConnection(_sqlConnectionString))
            {
                conn.Open();

                EnsureTableExists(conn, _tableName);
            }
        }

        public ISingleThreadTapeWriter GetOrCreateWriter(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var writer = _writers.GetOrAdd(
                name,
                n => new SingleThreadSqlTapeWriter(_sqlConnectionString, _tableName, name));

            return writer;
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
        [Index] [bigint] NOT NULL,
        [Data] [varbinary] (max) NOT NULL
        PRIMARY KEY CLUSTERED ([Stream], [Index])
    )
END";
            using (var cmd = new SqlCommand(string.Format(cmdText, TableSchema, tableName), conn))
            {
                cmd.Parameters.AddWithValue("@Schema", TableSchema);
                cmd.Parameters.AddWithValue("@Catalog", conn.Database);
                cmd.Parameters.AddWithValue("@Name", tableName);

                cmd.ExecuteNonQuery();
            }
        }
    }
}