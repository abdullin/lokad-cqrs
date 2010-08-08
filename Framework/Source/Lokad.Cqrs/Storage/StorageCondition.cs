using System;
using System.Globalization;
using Lokad.Rules;

namespace Lokad.Cqrs
{
	public struct StorageCondition
	{
		readonly string _etag;
		readonly DateTime _lastModifiedUtc;

		public StorageConditionType Type { get; private set; }
		public Maybe<string> ETag { get { return _etag ?? Maybe<string>.Empty;} }
		public Maybe<DateTime> LastModifiedUtc { get { return _lastModifiedUtc == DateTime.MinValue ? Maybe<DateTime>.Empty : _lastModifiedUtc; } }
		

		public StorageCondition(StorageConditionType type, string eTag) : this()
		{
			Enforce.Argument(() => type, Is.NotDefault);
			Enforce.ArgumentNotEmpty(() => eTag);

			Type = type;
			_etag = eTag;
		}

		public StorageCondition(StorageConditionType type, DateTime lastModifiedUtc)
			: this()
		{
			Enforce.Argument(() => type, Is.NotDefault);
			Enforce.Argument(() => lastModifiedUtc, Is.NotDefault);

			Type = type;
			_lastModifiedUtc = lastModifiedUtc;
		}



		public static StorageCondition IfMatch(string tag)
		{
			return new StorageCondition(StorageConditionType.IfMatch, tag);
		}

		public static StorageCondition IfModifiedSince(DateTime lastModifiedUtc)
		{
			return new StorageCondition(StorageConditionType.IfModifiedSince, lastModifiedUtc);
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
	}
}