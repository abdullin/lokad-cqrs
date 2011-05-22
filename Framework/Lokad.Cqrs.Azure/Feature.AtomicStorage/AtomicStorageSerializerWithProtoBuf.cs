using System.IO;
using ProtoBuf;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageSerializerWithProtoBuf : IAtomicStorageSerializer
    {
        public void Serialize<TView>(TView view, Stream stream)
        {
            Serializer.Serialize(stream, view);
        }

        public TView Deserialize<TView>(Stream stream)
        {
            return Serializer.Deserialize<TView>(stream);
        }
    }
}