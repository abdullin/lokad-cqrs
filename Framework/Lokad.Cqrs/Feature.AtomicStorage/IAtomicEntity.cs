#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Feature.AtomicStorage
{
    /// <summary>
    /// View entity that has an identity (there can be many views
    /// of this type)
    /// </summary>
// ReSharper disable UnusedTypeParameter
    public interface IAtomicEntity<TKey> : IAtomicEntity
// ReSharper restore UnusedTypeParameter
    {
    }

    /// <summary>
    /// Base marker interface for the views, used internally to
    /// simplify assembly-based lookups
    /// </summary>
    public interface IAtomicEntity
    {
    }
}