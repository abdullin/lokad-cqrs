using System;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SqlTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        readonly string _sqlConnectionString;

        public SqlTapeWriterFactory(string sqlConnectionString)
        {
            _sqlConnectionString = sqlConnectionString;
        }

        public void Init()
        {
            // create DB if not exist
        }

        public ISingleThreadTapeWriter GetOrCreateWriter(string name)
        {
            throw new NotImplementedException();
        }
    }
}