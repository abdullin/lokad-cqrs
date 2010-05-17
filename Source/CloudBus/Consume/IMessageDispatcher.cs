#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace CloudBus.Consume
{
	public interface IMessageDispatcher
	{
		void Init();
		bool DispatchMessage(string topic, object message);
	}
}