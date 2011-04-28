#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendIAtomicSingletonWriter
    {
        public static void EnforceAndUpdate<TView>(this IAtomicSingletonWriter<TView> self, Action<TView> update)
            where TView : IAtomicSingleton, new()
        {
            self.AddOrUpdate(() =>
                {
                    var view = new TView();
                    update(view);
                    return view;
                }, update);
        }

        public static void EnforceAndUpdate<TKey, TView>(this IAtomicEntityWriter<TKey, TView> self, TKey key,
            Action<TView> update)
            where TView : IAtomicEntity<TKey>, new()
        {
            self.AddOrUpdate(key, () =>
                {
                    var view = new TView();
                    update(view);
                    return view;
                }, update);
        }
    }
}