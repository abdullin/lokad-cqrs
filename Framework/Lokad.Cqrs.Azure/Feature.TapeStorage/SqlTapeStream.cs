using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class SqlTapeStream : ITapeStream
    {
        readonly string _connectionString;
        readonly string _tableName;
        readonly string _name;

        public SqlTapeStream(string connectionString, string tableName, string name)
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

            return Execute(c => ReadRecords(c, offset, maxCount), Enumerable.Empty<TapeRecord>());
        }

        public long GetVersion()
        {
            return Execute(GetCount, 0);
        }

        T Execute<T>(Func<SqlConnection, T> func, T defaultValue)
        {
            SqlConnection connection;
            try
            {
                connection = new SqlConnection(_connectionString);
                connection.Open();
            }
            catch (SqlException)
            {
                return defaultValue;
            }

            try
            {
                return func(connection);
            }
            finally
            {
                connection.Dispose();
            }
        }

        public void SaveRecords(IEnumerable<byte[]> records)
        {
            if (records == null)
                throw new ArgumentNullException("records");

            if (!records.Any())
                return;

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                var index = GetLastIndex(connection);

                foreach (var record in records)
                {
                    if (record.Length == 0)
                        throw new ArgumentException("Record must contain at least one byte.");

                    if (index > long.MaxValue - 1)
                        throw new IndexOutOfRangeException("Index is more than long.MaxValue.");
                    index++;

                    Append(connection, index, record);
                }
            }
        }

        void Append(SqlConnection connection, long index, byte[] record)
        {
            const string text = @"
INSERT INTO [{0}].[{1}] ([Stream], [Index], [Data])
VALUES (@Stream, @Index, @Data)";

            using (var command = new SqlCommand(string.Format(text, SqlTapeStorageFactory.TableSchema, _tableName), connection))
            {
                command.Parameters.AddWithValue("@Stream", _name);
                command.Parameters.AddWithValue("@Index", index);
                command.Parameters.AddWithValue("@Data", record);

                command.ExecuteNonQuery();
            }
        }

        long GetLastIndex(SqlConnection connection)
        {
            const string text = "SELECT Max([Index]) FROM [{0}].[{1}] WHERE [Stream] = @Stream";

            using (var command = new SqlCommand(string.Format(text, SqlTapeStorageFactory.TableSchema, _tableName), connection))
            {
                command.Parameters.AddWithValue("@Stream", _name);

                var result = command.ExecuteScalar();
                return result is DBNull ? -1 : (long)result;
            }
        }

        IEnumerable<TapeRecord> ReadRecords(SqlConnection connection, long offset, int count)
        {
            const string text = @"
SELECT TOP(@count) [Index], [Data]
FROM [{0}].[{1}]
WHERE [Stream] = @Stream AND [Index] >= (@offset)
ORDER BY [Index]";

            using (var command = new SqlCommand(string.Format(text, SqlTapeStorageFactory.TableSchema, _tableName), connection))
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

        long GetCount(SqlConnection connection)
        {
            const string text = "SELECT Max([Index]) FROM [{0}].[{1}] WHERE [Stream] = @Stream";

            using (var command = new SqlCommand(string.Format(text, SqlTapeStorageFactory.TableSchema, _tableName), connection))
            {
                command.Parameters.AddWithValue("@Stream", _name);

                var result = command.ExecuteScalar();
                var maxIndex = result is DBNull ? -1 : (long)result;

                if (maxIndex == long.MaxValue)
                    throw new OverflowException("Count is more than long.MaxValue");
                return maxIndex + 1;
            }
        }
    }
}