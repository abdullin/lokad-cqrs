#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class DefaultAtomicStorageStrategy : IAtomicStorageStrategy
    {
        readonly Type[] _entityTypes;
        readonly Type[] _singletonTypes;

        readonly string _folderForSingleton;
        readonly Func<Type, string> _nameForSingleton;
        readonly Func<Type, string> _folderForEntity;
        readonly Func<Type, object, string> _nameForEntity;
        readonly IAtomicStorageSerializer _serializer = new AtomicStorageSerializerWithDataContracts();


        public DefaultAtomicStorageStrategy(Type[] entityTypes, Type[] singletonTypes, string folderForSingleton,
            Func<Type, string> nameForSingleton, Func<Type, string> folderForEntity,
            Func<Type, object, string> nameForEntity, IAtomicStorageSerializer serializer)
        {
            _entityTypes = entityTypes;
            _serializer = serializer;
            _nameForEntity = nameForEntity;
            _folderForEntity = folderForEntity;
            _nameForSingleton = nameForSingleton;
            _folderForSingleton = folderForSingleton;
            _singletonTypes = singletonTypes;
        }

        public string GetFolderForEntity(Type entityType)
        {
            return _folderForEntity(entityType);
        }

        public string GetFolderForSingleton()
        {
            return _folderForSingleton;
        }

        public string GetNameForEntity(Type entity, object key)
        {
            return _nameForEntity(entity, key);
        }

        public string GetNameForSingleton(Type singletonType)
        {
            return _nameForSingleton(singletonType);
        }

        public void Serialize<TEntity>(TEntity entity, Stream stream)
        {
            _serializer.Serialize(entity, stream);
        }

        public TEntity Deserialize<TEntity>(Stream stream)
        {
            return _serializer.Deserialize<TEntity>(stream);
        }

        public Type[] GetEntityTypes()
        {
            return _entityTypes;
        }

        public Type[] GetSingletonTypes()
        {
            return _singletonTypes;
        }
    }
}