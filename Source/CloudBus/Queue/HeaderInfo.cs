using System;
using System.Runtime.Serialization;

namespace Bus2.Queue
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