#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;
using System.Linq;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public struct StorageCondition
    {
        public static StorageCondition None = default(StorageCondition);
        readonly string _etag;
        readonly DateTime? _lastModifiedUtc;

        public StorageCondition(StorageConditionType type, string eTag) : this()
        {
            if (eTag == null) throw new ArgumentNullException("eTag");
            if (type == StorageConditionType.None) throw new ArgumentException("type");
            if (string.IsNullOrEmpty(eTag)) throw new ArgumentException("eTag");

            Type = type;
            _etag = eTag;
        }

        public StorageCondition(StorageConditionType type, DateTime lastModifiedUtc)
            : this()
        {
            if (type == StorageConditionType.None) throw new ArgumentException("type");

            Type = type;
            _lastModifiedUtc = lastModifiedUtc;
        }

        public StorageConditionType Type { get; private set; }

        public Maybe<string> ETag
        {
            get { return _etag ?? Maybe<string>.Empty; }
        }

        public Maybe<DateTime> LastModifiedUtc
        {
            get { return _lastModifiedUtc ?? Maybe<DateTime>.Empty; }
        }


        /// <summary>
        /// <see cref="StorageConditionType.IfMatch"/>
        /// </summary>
        /// <param name="tag">The tag to use in constructing this condition.</param>
        /// <returns>new storage condition</returns>
        public static StorageCondition IfMatch(string tag)
        {
            return new StorageCondition(StorageConditionType.IfMatch, tag);
        }

        public static StorageCondition IfModifiedSince(DateTime lastModifiedUtc)
        {
            return new StorageCondition(StorageConditionType.IfModifiedSince, lastModifiedUtc);
        }

        public static StorageCondition IfUnmodifiedSince(DateTime lastModifiedUtc)
        {
            return new StorageCondition(StorageConditionType.IfUnmodifiedSince, lastModifiedUtc);
        }

        public static StorageCondition IfNoneMatch(string tag)
        {
            return new StorageCondition(StorageConditionType.IfNoneMatch, tag);
        }

        public override string ToString()
        {
            switch (Type)
            {
                case StorageConditionType.None:
                    return "None";
                case StorageConditionType.IfModifiedSince:
                case StorageConditionType.IfUnmodifiedSince:
                    return string.Format(CultureInfo.InvariantCulture, "{0} '{1}'", Type, _lastModifiedUtc);
                case StorageConditionType.IfMatch:
                case StorageConditionType.IfNoneMatch:
                    return string.Format(CultureInfo.InvariantCulture, "{0} '{1}'", Type, _etag);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool Satisfy(params LocalStorageInfo[] info)
        {
            // currently we have only a single match condition
            switch (Type)
            {
                case StorageConditionType.None:
                    // ???
                    return true;
                case StorageConditionType.IfUnmodifiedSince:
                    var j = LastModifiedUtc.Value;
                    return !info.Any(i => i.LastModifiedUtc > j);
                case StorageConditionType.IfMatch:
                    if (_etag == "*")
                        return info.Any();
                    var value = ETag.Value;
                    return info.Any(s => s.ETag == value);
                case StorageConditionType.IfModifiedSince:
                    var k = LastModifiedUtc.Value;
                    return info.Any(i => i.LastModifiedUtc > k);
                case StorageConditionType.IfNoneMatch:
                    if (_etag == "*")
                        return !info.Any();
                    var x = ETag.Value;
                    return !info.Any(s => s.ETag == x);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}