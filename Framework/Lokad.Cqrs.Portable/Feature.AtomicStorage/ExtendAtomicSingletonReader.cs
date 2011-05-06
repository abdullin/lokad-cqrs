#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendAtomicSingletonReader
    {
        public static TView GetOrNew<TView>(this IAtomicSingletonReader<TView> reader)
            where TView : new()
        {
            TView view;
            if (reader.TryGet(out view))
            {
                return view;
            }
            return new TView();
        }

        public static Optional<TSingleton> Get<TSingleton>(this IAtomicSingletonReader<TSingleton> reader)
        {
            TSingleton singleton;
            if (reader.TryGet(out singleton))
            {
                return singleton;
            }
            return Optional<TSingleton>.Empty;
        }
    }
}