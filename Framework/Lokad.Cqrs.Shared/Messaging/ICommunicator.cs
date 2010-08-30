#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Xml;
using Lokad.Messaging;

namespace Lokad.Cqrs.Messaging
{
	/// <summary>
	/// Real-time notification interface
	/// </summary>
	public interface ICommunicator : IObservable<ICommunicatorMessage>
	{
		/// <summary>
		/// Notifies the specified recipient (reliability is determined by the implementation.
		/// </summary>
		/// <param name="recipient">The recipient.</param>
		/// <param name="body">The body.</param>
		/// <param name="options">The options.</param>
		void Notify(string recipient, string body, CommunicationType options = CommunicationType.Chat);
	}

	public interface ICommunicatorMessage
	{
		string Subject { get; }
		string Body { get; }
		string Thread { get; }
		string Sender { get; }

		CommunicationType Type { get; }
	}
}