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

        public IEnumerable<TapeRecord> ReadRecords(int index, int maxCount)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("Must be more than or equal to zero.", "index");

            if (maxCount <= 0)
                throw new ArgumentOutOfRangeException("Must be more than zero.", "maxCount");

            if ((long) index + maxCount > (long) int.MaxValue + 1)
                throw new ArgumentOutOfRangeException("maxCount", "Record index will exceed int.MaxValue.");

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                return ReadRecords(connection, index, index + maxCount);
            }
        }

        IEnumerable<TapeRecord> ReadRecords(SqlConnection connection, int begin, int end)
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
                    if (index > int.MaxValue)
                        throw new IndexOutOfRangeException("Index is more than int.MaxValue.");

                    var data = (byte[]) reader["Data"];

                    records.Add(new TapeRecord((int) index, data));
                }

                return records;
            }
        }
    }
}