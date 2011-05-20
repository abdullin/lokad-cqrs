using System;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageSerializer
    {
        void Serialize<TView>(TView view, Stream stream);
        TView Deserialize<TView>(Stream stream);
    }
}