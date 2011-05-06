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
        /// Optional empty string
        /// </summary>
        public static readonly Maybe<string> String = Maybe<string>.Empty;
    }
}