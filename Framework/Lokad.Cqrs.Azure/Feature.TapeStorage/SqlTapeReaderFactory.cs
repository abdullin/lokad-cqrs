using System;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class SqlTapeReaderFactory : ITapeReaderFactory
    {
        readonly string _connectionString;


        public ITapeReader GetReader(string name)
        {
            throw new NotImplementedException();
        }
    }
}