#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    public static class ExtendIAtomicSingletonReader
    {
        public static TView GetOrNew<TView>(this IAtomicSingletonReader<TView> reader)
            where TView : new()
        {
            return reader.Get().GetValue(() => new TView());
        }
    }
}