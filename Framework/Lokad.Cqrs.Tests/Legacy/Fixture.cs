#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using Lokad.Cqrs.Core.Serialization;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Legacy
{
    public abstract class Fixture
    {
        static readonly ConcurrentDictionary<Type, IFormatter> Dict = new ConcurrentDictionary<Type, IFormatter>();

        static IFormatter GetFormatter(Type type)
        {
            return Dict.GetOrAdd(type, ProtoBufUtil.CreateFormatter);
        }

        protected T RoundTrip<T>(T item)
        {
            var formatter = GetFormatter(typeof (T));
            using (var memory = new MemoryStream())
            {
                formatter.Serialize(memory, item);
                memory.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(memory);
            }
        }

        protected T RoundTrip<T>(T item, Type legacy)
        {
            var formatter = GetFormatter(typeof (T));
            var via = GetFormatter(legacy);

            object intermediate;
            using (var memory = new MemoryStream())
            {
                formatter.Serialize(memory, item);
                memory.Seek(0, SeekOrigin.Begin);
                intermediate = via.Deserialize(memory);
            }

            using (var memory = new MemoryStream())
            {
                via.Serialize(memory, intermediate);
                memory.Seek(0, SeekOrigin.Begin);
                return (T) formatter.Deserialize(memory);
            }
        }
    }
}