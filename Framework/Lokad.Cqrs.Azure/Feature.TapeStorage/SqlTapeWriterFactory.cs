using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SqlTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        public const string TableName = "Cqrs_Tape_Storage";
        public const string TableSchema = "dbo";

        readonly string _sqlConnectionString;
        readonly ConcurrentDictionary<string, ISingleThreadTapeWriter> _writers =
            new ConcurrentDictionary<string, ISingleThreadTapeWriter>();

        public SqlTapeWriterFactory(string sqlConnectionString)
        {
            _sqlConnectionString = sqlConnectionString;
        }

        public void Init()
        {
            using (var conn = new SqlConnection(_sqlConnectionString))
            {
                conn.Open();

                EnsureTableExists(conn, TableName);
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
                n => new SingleThreadSqlTapeWriter(_sqlConnectionString, TableName, name));

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