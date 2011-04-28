#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Helper routines for <see cref="Maybe{T}"/>
    /// </summary>
    public static class Maybe
    {
        /// <summary>
        /// Creates new <see cref="Maybe{T}"/> from the provided value
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="item">The item.</param>
        /// <returns><see cref="Maybe{T}"/> that matches the provided value</returns>
        /// <exception cref="ArgumentNullException">if argument is a null reference</exception>
        public static Maybe<TSource> From<TSource>(TSource item)
        {
            // ReSharper disable CompareNonConstrainedGenericWithNull
            if (null == item) throw new ArgumentNullException("item");
            // ReSharper restore CompareNonConstrainedGenericWithNull

            return new Maybe<TSource>(item);
        }

        /// <summary>
        /// Optional empty boolean
        /// </summary>
        public static readonly Maybe<bool> Bool = Maybe<bool>.Empty;

        /// <summary>
        /// Optional empty string
        /// </summary>
        public static readonly Maybe<string> String = Maybe<string>.Empty;

        public static readonly Maybe<int> Int32 = Maybe<int>.Empty;
        public static readonly Maybe<long > Int64 = Maybe<long>.Empty;
    }
}