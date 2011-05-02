#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion


namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// Strongly-typed reader for the view singletons.
    /// </summary>
    /// <typeparam name="TSingleton">The type of the view.</typeparam>
    public interface IAtomicSingletonReader<TSingleton> 
    {
        /// <summary>
        /// Gets view singleton (if it's available).
        /// </summary>
        /// <returns>View singleton (if it's available)</returns>
        bool TryGet(out TSingleton singleton);
    }
}