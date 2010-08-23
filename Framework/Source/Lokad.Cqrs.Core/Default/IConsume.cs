#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Default
{
	/// <summary>
	/// Default CQRS interface for interface-base domain setup
	/// </summary>
	public interface IConsume<in TMessage> : IConsumeMessage
		where TMessage : IMessage
	{
		/// <summary>
		/// Consumes the specified message.
		/// </summary>
		/// <param name="message">The message.</param>
		void Consume(TMessage message);
	}
}