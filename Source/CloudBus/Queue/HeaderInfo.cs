#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Runtime.Serialization;

namespace CloudBus.Queue
{
	[DataContract]
	[Serializable]
	public class HeaderInfo
	{
		[DataMember] public readonly string Key;

		[DataMember] public readonly string Value;

		public HeaderInfo(string key, string value)
		{
			Key = key;
			Value = value;
		}
	}
}