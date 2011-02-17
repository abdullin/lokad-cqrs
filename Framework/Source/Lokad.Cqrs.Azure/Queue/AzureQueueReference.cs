#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

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

		public AzureQueueReference SubQueue(string type)
		{
			var name = QueueName + "-" + type;
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