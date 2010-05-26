#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;

namespace CloudBus.Domain
{
	public sealed class DomainMessageMapping
	{
		public readonly Type Consumer;
		public readonly Type Message;

		public readonly bool ConsumerCouldBeRegisteredInContainer;

		public DomainMessageMapping(Type message, Type consumer)
		{
			Message = message;
			Consumer = consumer;

			ConsumerCouldBeRegisteredInContainer = !consumer.IsAbstract;
		}

		public abstract class BusNull
		{
		}

		public abstract class BusSystem
		{
		}
	}
}