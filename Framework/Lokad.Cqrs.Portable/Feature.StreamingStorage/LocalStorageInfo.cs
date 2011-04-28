#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public sealed class LocalStorageInfo
    {
        public readonly DateTime LastModifiedUtc;
        public readonly string ETag;

        public LocalStorageInfo(DateTime lastModifiedUtc, string eTag)
        {
            LastModifiedUtc = lastModifiedUtc;
            ETag = eTag;
        }
    }
}