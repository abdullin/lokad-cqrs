using System;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public sealed class FileTapeReaderFactory : ITapeReaderFactory 
    {
        public ITapeReader GetReader(string name)
        {
            throw new NotImplementedException();
        }
    }
}