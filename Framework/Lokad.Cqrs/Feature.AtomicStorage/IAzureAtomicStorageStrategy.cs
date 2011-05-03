#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;
using System.IO;
using ProtoBuf;
using System.Linq;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAzureAtomicStorageStrategy 
    {
        string GetFolderForEntity(Type entityType);
        string GetFolderForSingleton();
        string GetNameForEntity(Type entity, object key);
        string GetNameForSingleton(Type singletonType);
        byte[] Serialize<TEntity>(TEntity entity);
        TEntity Deserialize<TEntity>(byte[] source);
        Type[] GetEntityTypes();
        Type[] GetSingletonTypes();
    }

    public sealed class DefaultAzureAtomicStorageStrategy : IAzureAtomicStorageStrategy
    {
        public string GetFolderForEntity(Type entityType)
        {
            return entityType.Name.ToLowerInvariant();
        }

        public string GetFolderForSingleton()
        {
            return "singletons";
        }

        public string GetNameForEntity(Type entity, object key)
        {
            return Convert.ToString(key, CultureInfo.InvariantCulture).ToLowerInvariant();
        }

        public string GetNameForSingleton(Type singletonType)
        {
            return singletonType.Name.ToLowerInvariant();
        }

        public byte[] Serialize<TEntity>(TEntity entity)
        {
            using (var memory = new MemoryStream())
            {
                Serializer.Serialize(memory, entity);
                return memory.ToArray();
            }
        }

        public TEntity Deserialize<TEntity>(byte[] source)
        {
            using (var memory = new MemoryStream(source))
            {
                return Serializer.Deserialize<TEntity>(memory);
            }
        }

        public Type[] GetEntityTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetExportedTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => typeof (Define.AtomicEntity).IsAssignableFrom(t))
                .ToArray();
        }

        public Type[] GetSingletonTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(t => t.GetExportedTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => typeof(Define.AtomicSingleton).IsAssignableFrom(t))
                .ToArray();
        }
    }
}