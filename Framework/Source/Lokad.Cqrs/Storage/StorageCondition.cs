using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Lokad.Rules;

namespace Lokad.Cqrs.Storage
{
	public struct StorageCondition
	{
		readonly string _etag;
		readonly DateTime? _lastModifiedUtc;

		public StorageConditionType Type { get; private set; }
		public Maybe<string> ETag { get { return _etag ?? Maybe<string>.Empty;} }
		public Maybe<DateTime> LastModifiedUtc { get { return _lastModifiedUtc ?? Maybe<DateTime>.Empty; } }
		

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
			//Enforce.Argument(() => lastModifiedUtc, Is.NotDefault);

			Type = type;
			_lastModifiedUtc = lastModifiedUtc;
		}

		public static StorageCondition None = default(StorageCondition);


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