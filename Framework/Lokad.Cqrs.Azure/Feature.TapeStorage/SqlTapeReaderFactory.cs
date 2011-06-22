using System;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SqlTapeReaderFactory : ITapeReaderFactory
    {
        readonly string _sqlConnectionString;
        readonly string _tableName;

        public SqlTapeReaderFactory(string sqlConnectionString, string tableName)
        {
            _sqlConnectionString = sqlConnectionString;
            _tableName = tableName;
        }

        public ITapeReader GetReader(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            return new SqlTapeReader(_sqlConnectionString, _tableName, name);
        }
    }
}