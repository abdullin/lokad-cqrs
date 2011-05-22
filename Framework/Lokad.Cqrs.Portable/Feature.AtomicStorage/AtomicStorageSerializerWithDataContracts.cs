using System.IO;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageSerializerWithDataContracts : IAtomicStorageSerializer
    {
        static class Cache<T>
        {
            public static readonly DataContractSerializer Serializer = new DataContractSerializer(typeof(T));
        }
        
        public void Serialize<TView>(TView view, Stream stream)
        {
            Cache<TView>.Serializer.WriteObject(stream, view);
        }

        public TView Deserialize<TView>(Stream stream)
        {
            return (TView) Cache<TView>.Serializer.ReadObject(stream);
        }
    }
}