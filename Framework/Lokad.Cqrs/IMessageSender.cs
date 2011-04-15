#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Generic message publishing interface that is provided by the infrastructure, should user configure it for publishing
	/// </summary>
	public interface IMessageSender
	{
		/// <summary>
		/// Sends the specified messages to the designated recipient.
		/// </summary>
		/// <param name="message">The message to send.</param>
		void Send(object message);

		void DelaySend(TimeSpan timeout, object message);


		void SendBatch(params object[] messageItems);
		void DelaySendBatch(TimeSpan timeout, params object[] messageItems);


	}
}