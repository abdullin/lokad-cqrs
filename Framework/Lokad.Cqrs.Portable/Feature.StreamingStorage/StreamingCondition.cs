#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;
using System.Linq;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    public struct StreamingCondition
    {
        public static StreamingCondition None = default(StreamingCondition);
        readonly string _etag;
        readonly DateTime? _lastModifiedUtc;

        public StreamingCondition(StreamingConditionType type, string eTag) : this()
        {
            if (eTag == null) throw new ArgumentNullException("eTag");
            if (type == StreamingConditionType.None) throw new ArgumentException("type");
            if (string.IsNullOrEmpty(eTag)) throw new ArgumentException("eTag");

            Type = type;
            _etag = eTag;
        }

        public StreamingCondition(StreamingConditionType type, DateTime lastModifiedUtc)
            : this()
        {
            if (type == StreamingConditionType.None) throw new ArgumentException("type");

            Type = type;
            _lastModifiedUtc = lastModifiedUtc;
        }

        public StreamingConditionType Type { get; private set; }

        public Optional<string> ETag
        {
            get { return _etag ?? Optional<string>.Empty; }
        }

        public Optional<DateTime> LastModifiedUtc
        {
            get { return _lastModifiedUtc ?? Optional<DateTime>.Empty; }
        }


        /// <summary>
        /// <see cref="StreamingConditionType.IfMatch"/>
        /// </summary>
        /// <param name="tag">The tag to use in constructing this condition.</param>
        /// <returns>new storage condition</returns>
        public static StreamingCondition IfMatch(string tag)
        {
            return new StreamingCondition(StreamingConditionType.IfMatch, tag);
        }

        public static StreamingCondition IfModifiedSince(DateTime lastModifiedUtc)
        {
            return new StreamingCondition(StreamingConditionType.IfModifiedSince, lastModifiedUtc);
        }

        public static StreamingCondition IfUnmodifiedSince(DateTime lastModifiedUtc)
        {
            return new StreamingCondition(StreamingConditionType.IfUnmodifiedSince, lastModifiedUtc);
        }

        public static StreamingCondition IfNoneMatch(string tag)
        {
            return new StreamingCondition(StreamingConditionType.IfNoneMatch, tag);
        }

        public override string ToString()
        {
            switch (Type)
            {
                case StreamingConditionType.None:
                    return "None";
                case StreamingConditionType.IfModifiedSince:
                case StreamingConditionType.IfUnmodifiedSince:
                    return string.Format(CultureInfo.InvariantCulture, "{0} '{1}'", Type, _lastModifiedUtc);
                case StreamingConditionType.IfMatch:
                case StreamingConditionType.IfNoneMatch:
                    return string.Format(CultureInfo.InvariantCulture, "{0} '{1}'", Type, _etag);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool Satisfy(params LocalStreamingInfo[] info)
        {
            // currently we have only a single match condition
            switch (Type)
            {
                case StreamingConditionType.None:
                    // ???
                    return true;
                case StreamingConditionType.IfUnmodifiedSince:
                    var j = LastModifiedUtc.Value;
                    return !info.Any(i => i.LastModifiedUtc > j);
                case StreamingConditionType.IfMatch:
                    if (_etag == "*")
                        return info.Any();
                    var value = ETag.Value;
                    return info.Any(s => s.ETag == value);
                case StreamingConditionType.IfModifiedSince:
                    var k = LastModifiedUtc.Value;
                    return info.Any(i => i.LastModifiedUtc > k);
                case StreamingConditionType.IfNoneMatch:
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