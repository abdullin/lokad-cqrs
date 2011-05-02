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
            return self.AddOrUpdate(() =>
                {
                    var view = new TView();
                    update(view);
                    return view;
                }, update, hint);
        }
    }
}