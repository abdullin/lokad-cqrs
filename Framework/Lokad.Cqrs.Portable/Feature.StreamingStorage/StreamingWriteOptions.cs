#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    [Flags]
    public enum StreamingWriteOptions
    {
        None,
        /// <summary>
        /// We'll compress data if possible.
        /// </summary>
        CompressIfPossible = 0x01,
        /// <summary>
        /// Be default we are optimizing for small read operations. Use this as a hint
        /// </summary>
        OptimizeForLargeWrites = 0x02
    }
}