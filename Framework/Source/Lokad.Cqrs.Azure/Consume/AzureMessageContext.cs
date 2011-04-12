#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using Lokad.Cqrs.Durability;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Consume
{
	public sealed class AzureMessageContext
	{
		public readonly CloudQueueMessage CloudMessage;
		public readonly MessageEnvelope Unpacked;

		public AzureMessageContext(CloudQueueMessage cloudMessage, MessageEnvelope unpacked)
		{
			CloudMessage = cloudMessage;
			Unpacked = unpacked;
		}
	}
}