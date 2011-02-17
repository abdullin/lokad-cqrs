#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs.Queue
{
	static class QueueErrors
	{
		public static Exception NoContractNameOnSend(Type messageType, IMessageSerializer serializer)
		{
			return Errors.InvalidOperation(
					"Can't find contract name to serialize message: '{0}'. Make sure that your message types are loaded by domain and are compatible with '{1}'.",
					messageType, serializer.GetType().Name);
		}
	}
}