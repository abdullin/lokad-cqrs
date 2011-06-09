using System;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SqlTapeReaderFactory : ITapeReaderFactory
    {
        readonly string _sqlConnectionString;

        public SqlTapeReaderFactory(string sqlConnectionString)
        {
            _sqlConnectionString = sqlConnectionString;
        }

        public ITapeReader GetReader(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            return new SingleThreadSqlTapeReader(_sqlConnectionString, SqlTapeWriterFactory.TableName, name);
        }
    }
}