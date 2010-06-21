#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;

namespace Lokad.Cqrs.Domain
{
	[DebuggerDisplay("{ConsumerType.Name}")]
	public sealed class ConsumerInfo
	{
		public readonly Type ConsumerType;
		public readonly Type[] MessageTypes;

		public ConsumerInfo(Type consumerType, Type[] messageTypes)
		{
			ConsumerType = consumerType;
			MessageTypes = messageTypes;
		}
	}
}