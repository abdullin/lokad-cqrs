#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Lokad.Cqrs.Evil;
using ProtoBuf;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class DefaultAzureAtomicStorageStrategy : IAzureAtomicStorageStrategy
    {
        readonly Predicate<Type> _entityFilter;
        readonly Predicate<Type> _singletonFilter;
        readonly ICollection<Assembly> _extraAssemblies;

        public DefaultAzureAtomicStorageStrategy(Predicate<Type> entityFilter, Predicate<Type> singletonFilter, ICollection<Assembly> extraAssemblies)
        {
            _entityFilter = entityFilter;
            _singletonFilter = singletonFilter;
            _extraAssemblies = extraAssemblies;
        }

        public string GetFolderForEntity(Type entityType)
        {
            return "atomic-" +entityType.Name.ToLowerInvariant();
        }

        public string GetFolderForSingleton()
        {
            return "atomic-singleton";
        }

        public string GetNameForEntity(Type entity, object key)
        {
            return Convert.ToString(key, CultureInfo.InvariantCulture).ToLowerInvariant() + ".pb";
        }

        public string GetNameForSingleton(Type singletonType)
        {
            return singletonType.Name.ToLowerInvariant() + ".pb";
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
                .Where(AssemblyScanEvil.IsUserAssembly)
                .Concat(_extraAssemblies)
                .Distinct()
                .SelectMany(t => t.GetExportedTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => _entityFilter(t))
                .ToArray();
        }

        public Type[] GetSingletonTypes()
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(AssemblyScanEvil.IsUserAssembly)
                .Concat(_extraAssemblies)
                .Distinct()
                .SelectMany(t => t.GetExportedTypes())
                .Where(t => !t.IsAbstract)
                .Where(t => _singletonFilter(t))
                .ToArray();
        }
    }
}