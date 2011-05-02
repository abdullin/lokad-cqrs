#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Strongly-typed view singleton writer
    /// </summary>
    /// <typeparam name="TSingleton">The type of the view.</typeparam>
    public interface IAtomicSingletonWriter<TSingleton> 
    {
        TSingleton AddOrUpdate(Func<TSingleton> addFactory, Func<TSingleton,TSingleton> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists);
        TSingleton AddOrUpdate(Func<TSingleton> addFactory, Action<TSingleton> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists);
        TSingleton UpdateOrThrow(Action<TSingleton> update);
        TSingleton UpdateOrThrow(Func<TSingleton, TSingleton> update);

        /// <summary>
        /// Deletes this view singleton.
        /// </summary>
        bool TryDelete();
    }

    public enum AddOrUpdateHint
    {
        ProbablyExists,
        ProbablyDoesNotExist
    }
}