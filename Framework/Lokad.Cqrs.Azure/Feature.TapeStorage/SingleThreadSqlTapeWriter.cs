using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class SingleThreadSqlTapeWriter : ISingleThreadTapeWriter
    {
        readonly string _connectionString;
        readonly string _tableName;
        readonly string _name;

        public SingleThreadSqlTapeWriter(string connectionString, string tableName, string name)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _name = name;
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

                var index = GetLastIndex(connection) + 1;

                foreach (var record in records)
                {
                    if (record.Length == 0)
                        throw new ArgumentException("Record must contain at least one byte.");

                    if (index > int.MaxValue)
                        throw new IndexOutOfRangeException("Index is more than int.MaxValue.");

                        Append(connection, (int) index, record);

                    index++;
                }
            }
        }

        void Append(SqlConnection connection, int index, byte[] record)
        {
            const string text = @"
INSERT INTO [{0}].[{1}] ([Stream], [Index], [Data])
VALUES (@Stream, @Index, @Data)";

            using (var command = new SqlCommand(string.Format(text, SqlTapeWriterFactory.TableSchema, _tableName), connection))
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

            using (var command = new SqlCommand(string.Format(text, SqlTapeWriterFactory.TableSchema, _tableName), connection))
            {
                command.Parameters.AddWithValue("@Stream", _name);

                var result = command.ExecuteScalar();
                return result is DBNull ? 0 : (long) result;
            }
        }
    }
}