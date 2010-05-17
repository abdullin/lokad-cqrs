#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;

namespace CloudBus.Domain
{
	[DebuggerDisplay("{MessageType.Name}")]
	public sealed class MessageInfo
	{
		public MessageInfo(Type messageType, Type[] consumers, bool isDomainMessage, bool isSystemMessage)
		{
			MessageType = messageType;
			IsSystemMessage = isSystemMessage;
			IsDomainMessage = isDomainMessage;
			DirectConsumers = consumers;

			IsInterface = messageType.IsAbstract;
			Implements = new MessageInfo[0];
		}

		public Type MessageType { get; private set; }
		public Type[] DirectConsumers { get; internal set; }
		public Type[] DerivedConsumers { get; internal set; }
		public Type[] AllConsumers { get; internal set; }

		public bool IsDomainMessage { get; private set; }
		public bool IsSystemMessage { get; private set; }
		public bool IsInterface { get; private set; }
		public MessageInfo[] Implements { get; internal set; }
	}
}