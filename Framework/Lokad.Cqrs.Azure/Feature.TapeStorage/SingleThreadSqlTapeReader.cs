using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class SingleThreadSqlTapeReader : ITapeReader
    {
        readonly string _connectionString;
        readonly string _tableName;
        readonly string _name;

        public SingleThreadSqlTapeReader(string connectionString, string tableName, string name)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _name = name;
        }

        public IEnumerable<TapeRecord> ReadRecords(long offset, int maxCount)
        {
            if (offset < 0)
                throw new ArgumentOutOfRangeException("Must be non-negative.", "offset");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Must be more than zero.", "maxCount");


            // index + maxCount - 1 > long.MaxValue, but transformed to avoid overflow
            if (offset > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return ReadRecords(connection, offset, maxCount);
            }
        }

        IEnumerable<TapeRecord> ReadRecords(SqlConnection connection, long offset, int count)
        {
            const string text = @"
SELECT TOP(@count) [Index], [Data]
FROM [{0}].[{1}]
WHERE [Stream] = @Stream AND [Index] >= (@offset)
ORDER BY [Index]";

            using (var command = new SqlCommand(string.Format(text, SqlTapeWriterFactory.TableSchema, _tableName), connection))
            {
                command.Parameters.AddWithValue("@Stream", _name);
                command.Parameters.AddWithValue("@count", count);
                command.Parameters.AddWithValue("@offset", offset);

                var reader = command.ExecuteReader();

                var records = new List<TapeRecord>();

                while (reader.Read())
                {
                    var index = (long) reader["Index"];
                    var data = (byte[]) reader["Data"];

                    records.Add(new TapeRecord(index, data));
                }

                return records;
            }
        }
    }
}