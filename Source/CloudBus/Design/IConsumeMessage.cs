#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace CloudBus
{
	public interface IConsumeMessage<TMessage> : IConsumeMessage
		where TMessage : IBusMessage
	{
		void Consume(TMessage message);
	}

	public interface IConsumeMessage{}
}