#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Lokad.Cqrs.Evil;
// ReSharper disable UnusedMember.Global
namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Allows to configure default implementation of <see cref="IAtomicStorageStrategy"/>
    /// </summary>
    public sealed class DefaultAtomicStorageStrategyBuilder : HideObjectMembersFromIntelliSense
    {
        Predicate<Type> _entityTypeFilter = type => typeof (Define.AtomicEntity).IsAssignableFrom(type);
        Predicate<Type> _singletonTypeFilter = type => typeof (Define.AtomicSingleton).IsAssignableFrom(type);
        readonly List<Assembly> _extraAssemblies = new List<Assembly>();

        string _folderForSingleton = "atomic-singleton";
        Func<Type, string> _nameForSingleton = type => CleanName(type.Name) + ".pb";
        Func<Type, string> _folderForEntity = type => CleanName("atomic-" +type.Name);
        Func<Type, object, string> _nameForEntity =
            (type, key) => (CleanName(type.Name) + "-" + Convert.ToString(key, CultureInfo.InvariantCulture).ToLowerInvariant()) + ".pb";

        IAtomicStorageSerializer _serializer;

        /// <summary>
        /// Provides custom folder for storing singletons.
        /// </summary>
        /// <param name="folderName">Name of the folder.</param>
        public void FolderForSingleton(string folderName)
        {
            _folderForSingleton = folderName;
        }

        /// <summary>
        /// Helper to clean the name, making it suitable for azure storage
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns></returns>
        public static string CleanName(string typeName)
        {
            var sb = new StringBuilder();

            bool lastWasUpper = false;
            bool lastWasSymbol = true;

            foreach (var c in typeName)
            {
                var splitRequired = char.IsUpper(c) || !char.IsLetterOrDigit(c);
                if (splitRequired && !lastWasUpper && !lastWasSymbol)
                {
                    sb.Append('-');
                }
                lastWasUpper = char.IsUpper(c);
                lastWasSymbol = !char.IsLetterOrDigit(c);

                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(char.ToLowerInvariant(c));
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Provides custom naming convention for the singleton files
        /// </summary>
        /// <param name="namingConvention">The naming convention.</param>
        public void NameForSingleton(Func<Type,string> namingConvention)
        {
            _nameForSingleton = namingConvention;
        }
        /// <summary>
        /// Provides custom naming convention for entity folders.
        /// </summary>
        /// <param name="namingConvention">The naming convention.</param>
        public void FolderForEntity(Func<Type,string> namingConvention)
        {
            _folderForEntity = namingConvention;
        }

        public void CustomStaticSerializer(IAtomicStorageSerializer serializer)
        {
            _serializer = serializer;
        }

        /// <summary>
        /// Provides custom naming convention for entity files.
        /// </summary>
        /// <param name="namingConvention">The naming convention.</param>
        public void NameForEntity(Func<Type,object,string> namingConvention)
        {
            _nameForEntity = namingConvention;
        }

        /// <summary>
        /// Specifies base entity type to use in assembly scans. Default is <see cref="Define.AtomicEntity"/>
        /// </summary>
        /// <typeparam name="TEntityBase">Base entity class from which all atomic entities are derived.</typeparam>
        public void WhereEntityIs<TEntityBase>()
        {
            _entityTypeFilter = type => typeof (TEntityBase).IsAssignableFrom(type);
        }

        /// <summary>
        /// Allows to specify completely custom search pattern for entity types. Default is to look for inheritors from 
        /// <see cref="Define.AtomicEntity"/>
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public void WhereEntity(Predicate<Type> predicate)
        {
            _entityTypeFilter = predicate;
        }

        /// <summary>
        /// Allows to specify completely cstom search pattern for singleton types. Default behavior is to look for
        /// inheritors from <see cref="Define.AtomicSingleton"/>
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        public void WhereSingleton(Predicate<Type> predicate)
        {
            _singletonTypeFilter = predicate;
        }
        /// <summary>
        /// Specifies base singleton type to use in assembly scans. Default is <see cref="Define.AtomicSingleton"/>
        /// </summary>
        /// <typeparam name="TSingletonBase">Base singleton class from which all atomic singletons are derived.</typeparam>
        public void WhereSingletonIs<TSingletonBase>()
        {
            _singletonTypeFilter = type => typeof (TSingletonBase).IsAssignableFrom(type);
        }

        /// <summary>
        /// Specifies an additional assembly to scan for atomic types (in addition to the loaded assemblies)
        /// </summary>
        /// <param name="assembly">The assembly to include into scan for atomic types.</param>
        public void WithAssembly(Assembly assembly)
        {
            _extraAssemblies.Add(assembly);
        }

        /// <summary>
        /// Specifies an additional assembly to scan for atomic types (in addition to the loaded assemblies)
        /// </summary>
        /// <typeparam name="T">type, located in assembly to include in scan</typeparam>
        public void WithAssemblyOf<T>()
        {
            _extraAssemblies.Add(typeof(T).Assembly);
        }

        /// <summary>
        /// Builds new instance of immutable <see cref="IAtomicStorageStrategy"/>
        /// </summary>
        /// <returns></returns>
        public IAtomicStorageStrategy Build()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(AssemblyScanEvil.IsUserAssembly)
                .Concat(_extraAssemblies)
                .Distinct()
                .SelectMany(t => t.GetExportedTypes())
                .Where(t => !t.IsAbstract)
                .ToArray();
            var entities = types
                .Where(t => _entityTypeFilter(t))
                .ToArray();

            var singletons = types
                .Where(t => _singletonTypeFilter(t))
                .ToArray();

            return new DefaultAtomicStorageStrategy(
                entities, 
                singletons, 
                _folderForSingleton, 
                _nameForSingleton, 
                _folderForEntity, 
                _nameForEntity, _serializer);
        }
    }
}