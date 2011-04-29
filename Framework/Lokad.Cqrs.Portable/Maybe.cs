#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs
{
    /// <summary>
    /// Helper routines for <see cref="Maybe{T}"/>
    /// </summary>
    public static class Maybe
    {
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