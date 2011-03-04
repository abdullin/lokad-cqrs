#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion


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
		/// <param name="messages">The messages to send.</param>
		void Send(params object[] messages);


		void SendAsBatch(params object[] messageItems);
	}
}