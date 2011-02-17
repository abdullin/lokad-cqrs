#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Cqrs.Directory;

using Lokad.Cqrs.Queue;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Domain
{
	
	public sealed class DomainAwareMessageProfiler : IMessageProfiler
	{
		readonly IDictionary<Type, GetInfoDelegate> _delegates;

		public DomainAwareMessageProfiler(MessageDirectory directory)
		{
			_delegates = BuildFrom(directory);
		}


		public string GetReadableMessageInfo(UnpackedMessage message)
		{
			GetInfoDelegate value;
			
			if (_delegates.TryGetValue(message.ContractType, out value))
			{
				return value(message);
			}
			return GetDefaultInfo(message);
		}

		static string GetDefaultInfo(UnpackedMessage message)
		{
			var contract = message.ContractType.Name;
			return message
				.GetState<CloudQueueMessage>()
				.Convert(s => contract + " - " + s.Id, contract);
		}

		static IDictionary<Type, GetInfoDelegate> BuildFrom(MessageDirectory directory)
		{
			var delegates = new Dictionary<Type, GetInfoDelegate>();
			foreach (var message in directory.Messages)
			{
				if (message.MessageType.IsInterface)
					continue;

				var type = message.MessageType;
				var hasStringOverride = type.GetMethod("ToString").DeclaringType != typeof (object);

				if (hasStringOverride)
				{
					delegates.Add(type, m => m.Content.ToString());
				}
				else
				{
					delegates.Add(type, GetDefaultInfo);
				}
			}
			return delegates;
		}

		delegate string GetInfoDelegate(UnpackedMessage message);
	}
}