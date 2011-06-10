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

        public IEnumerable<TapeRecord> ReadRecords(long index, int maxCount)
        {
            if (index <= 0)
                throw new ArgumentOutOfRangeException("Must be more than zero.", "index");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Must be more than zero.", "maxCount");

            // index + maxCount - 1 > long.MaxValue, but transformed to avoid overflow
            if (index - 1 > long.MaxValue - maxCount)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed long.MaxValue.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return ReadRecords(connection, index, index + maxCount);
            }
        }

        IEnumerable<TapeRecord> ReadRecords(SqlConnection connection, long begin, long end)
        {
            const string text = @"
SELECT [Index], [Data]
FROM [{0}].[{1}]
WHERE [Stream] = @Stream AND [Index] >= @Begin AND [Index] < @End
ORDER BY [Index]";

            using (var command = new SqlCommand(string.Format(text, SqlTapeWriterFactory.TableSchema, _tableName), connection))
            {
                command.Parameters.AddWithValue("@Stream", _name);
                command.Parameters.AddWithValue("@Begin", begin);
                command.Parameters.AddWithValue("@End", end);

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