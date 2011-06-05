using System;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class AtomicStorageSerializerWithDelegates : IAtomicStorageSerializer
    {
        readonly Action<object, Type, Stream> _serializer;
        readonly Func<Type, Stream, object> _deserializer;

        public AtomicStorageSerializerWithDelegates(Action<object, Type, Stream> serializer, Func<Type, Stream, object> deserializer)
        {
            _serializer = serializer;
            _deserializer = deserializer;
        }

        public void Serialize<TView>(TView view, Stream stream)
        {
            _serializer(view,typeof(TView), stream);
        }

        public TView Deserialize<TView>(Stream stream)
        {
            return (TView) _deserializer(typeof(TView),stream);
        }
    }
}