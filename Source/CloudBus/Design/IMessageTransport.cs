#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using CloudBus.Queue;

namespace CloudBus
{
	public interface IMessageTransport : IDisposable
	{
		int ThreadCount { get; }
		void Start();

		event Action Started;
		event Func<IncomingMessage, bool> MessageRecieved;
		event Action<IncomingMessage, Exception> MessageHandlerFailed;
	}
}