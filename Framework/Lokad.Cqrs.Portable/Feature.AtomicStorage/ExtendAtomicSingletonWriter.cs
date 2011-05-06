#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicSingletonWriter
    {
        

        public static TView AddOrUpdate<TView>(this IAtomicSingletonWriter<TView> self, Action<TView> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
            where TView : new()
        {
            return self.AddOrUpdate(() => new TView(), view =>
                {
                    update(view);
                    return view;
                }, hint);
        }

        public static TView AddOrUpdate<TView>(this IAtomicSingletonWriter<TView> self, Func<TView> factory, Action<TView> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists)
        {
            return self.AddOrUpdate(factory, view =>
            {
                update(view);
                return view;
            }, hint);
        }

        public static TEntity UpdateOrThrow<TEntity>(this IAtomicSingletonWriter<TEntity> self, Func<TEntity, TEntity> change)
        {
            return self.AddOrUpdate(() =>
            {
                var txt = String.Format("Failed to load '{0}'.", typeof(TEntity).Name);
                throw new InvalidOperationException(txt);
            }, change, AddOrUpdateHint.ProbablyExists);
        }
        public static TEntity UpdateOrThrow<TEntity>(this IAtomicSingletonWriter<TEntity> self, Action<TEntity> change)
        {
            return self.AddOrUpdate(() =>
            {
                var txt = String.Format("Failed to load '{0}'.", typeof(TEntity).Name);
                throw new InvalidOperationException(txt);
            }, view =>
                {
                    change(view);
                    return view;
                }, AddOrUpdateHint.ProbablyExists);
        }

        public static TSingleton UpdateEnforcingNew<TSingleton>(this IAtomicSingletonWriter<TSingleton> self, Action<TSingleton> update) where TSingleton : new()
        {
            return self.AddOrUpdate(() =>
            {
                var singleton = new TSingleton();
                update(singleton);
                return singleton;
            }, update);
        }


    }
}