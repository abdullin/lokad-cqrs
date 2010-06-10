#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs
{
	sealed class SimpleMessageProfiler : IMessageProfiler
	{
		public static readonly IMessageProfiler Instance = new SimpleMessageProfiler();

		public string GetReadableMessageInfo(object instance, string messageId)
		{
			return instance.GetType().Name + "- " + messageId;
		}
	}
}