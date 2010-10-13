#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using ExtendIEnumerable = Lokad.ExtendIEnumerable;

namespace Lokad.Cqrs.Queue
{
	public sealed class AzureQueueReference
	{
		public readonly Uri Endpoint;
		public readonly string QueueName;
		public readonly Uri Uri;


		public AzureQueueReference(Uri endpoint, string queueName)
		{
			Uri = new Uri(endpoint + "/" + queueName);
			QueueName = queueName;
			Endpoint = endpoint;
		}

		public static AzureQueueReference FromUri(Uri uri)
		{
			var segments = uri.Segments;
			var @join = ExtendIEnumerable.Join(segments.Take(segments.Length - 1), "");
			var endpoint = new Uri(uri, @join.TrimEnd('/'));
			return new AzureQueueReference(endpoint, segments.Last());
		}

		public AzureQueueReference SubQueue(SubQueueType type)
		{
			var name = QueueName + "-" + type.ToString().ToLowerInvariant();
			return new AzureQueueReference(Endpoint, name);
		}

		public bool Equals(AzureQueueReference other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.Uri, Uri);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (AzureQueueReference)) return false;
			return Equals((AzureQueueReference) obj);
		}

		public override int GetHashCode()
		{
			return (Uri != null ? Uri.GetHashCode() : 0);
		}
	}
}