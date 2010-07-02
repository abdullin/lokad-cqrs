#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Specialized;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Queue
{
	public sealed class UnpackedCloudMessage : UnpackedMessage
	{
		CloudQueueMessage _inner;
		public readonly string PopReceipt;

		public UnpackedCloudMessage(MessageHeader header, MessageAttribute[] attributes, object content, CloudQueueMessage inner, Type contract) : base(header, attributes, content, inner.Id, contract)
		{
			_inner = inner;

			PopReceipt = inner.PopReceipt;
		}
	}
}