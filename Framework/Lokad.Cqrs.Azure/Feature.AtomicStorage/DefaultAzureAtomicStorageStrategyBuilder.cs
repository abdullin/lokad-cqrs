#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public sealed class DefaultAzureAtomicStorageStrategyBuilder : HideObjectMembersFromIntelliSense
    {
        Predicate<Type> _entityTypeFilter = type => typeof (Define.AtomicEntity).IsAssignableFrom(type);
        Predicate<Type> _singletonTypeFilter = type => typeof (Define.AtomicSingleton).IsAssignableFrom(type);


        public void WhereEntityIs<TEntityBase>()
        {
            _entityTypeFilter = type => typeof (TEntityBase).IsAssignableFrom(type);
        }

        public void WhereEntity(Predicate<Type> predicate)
        {
            _entityTypeFilter = predicate;
        }

        public void WhereSingleton(Predicate<Type> predicate)
        {
            _singletonTypeFilter = predicate;
        }

        public void WhereSingletonIs<TSingletonBase>()
        {
            _singletonTypeFilter = type => typeof (TSingletonBase).IsAssignableFrom(type);
        }

        public IAzureAtomicStorageStrategy Build()
        {
            return new DefaultAzureAtomicStorageStrategy(_entityTypeFilter, _singletonTypeFilter);
        }
    }
}