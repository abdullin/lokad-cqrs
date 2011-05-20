#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public interface IAtomicStorageStrategy 
    {
        string GetFolderForEntity(Type entityType);
        string GetFolderForSingleton();
        string GetNameForEntity(Type entity, object key);
        string GetNameForSingleton(Type singletonType);
        void Serialize<TEntity>(TEntity entity, Stream stream);
        TEntity Deserialize<TEntity>(Stream stream);
        Type[] GetEntityTypes();
        Type[] GetSingletonTypes();
    }
}