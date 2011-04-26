#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Evil
{
    /// <summary>
    /// Helper methods for the <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class ExtendIEnumerable
    {
        /// <summary>
        /// Converts the enumerable to <see cref="HashSet{T}"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>hashset instance</returns>
        public static HashSet<T> ToSet<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException("enumerable");

            return new HashSet<T>(enumerable);
        }

        /// <summary>
        /// Concatenates a specified separator between each element of a specified <paramref name="strings"/>, 
        /// yielding a single concatenated string.
        /// </summary>
        /// <param name="strings">The strings.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>concatenated string</returns>
        public static string JoinStrings(this IEnumerable<string> strings, string separator)
        {
            if (strings == null) throw new ArgumentNullException("strings");

            return string.Join(separator, strings.ToArray());
        }
    }
}