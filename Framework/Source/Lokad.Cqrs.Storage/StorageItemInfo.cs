using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Lokad.Cqrs
{
	public sealed class StorageItemInfo
	{
		public DateTime LastModifiedUtc { get; private set; }
		public string ETag { get; private set; }

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public NameValueCollection Metadata { get; private set; }
		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public IDictionary<string,string> Properties { get; private set; }

		//public string FullPath { get; private set; }
		//public string Name { get; private set; }

		public StorageItemInfo(
			//string name,
			//string fullPath,
			DateTime lastModifiedUtc, 
			string eTag, 
			NameValueCollection metadata, 
			IDictionary<string,string> properties)
		{

			//Name = name;
			//FullPath = fullPath;

			LastModifiedUtc = lastModifiedUtc;
			ETag = eTag;
			Metadata = metadata;
			Properties = properties;
		}
	}
}