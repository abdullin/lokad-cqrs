#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Lokad.Cqrs.Queue;

namespace Lokad.Cqrs
{
	public interface IMessageTransport : IDisposable
	{
		int ThreadCount { get; }
		void Start();

		event Action Started;
		event Action Stopped;
		event Func<UnpackedMessage, bool> MessageReceived;
		event Action<UnpackedMessage, Exception> MessageHandlerFailed;
	}
}