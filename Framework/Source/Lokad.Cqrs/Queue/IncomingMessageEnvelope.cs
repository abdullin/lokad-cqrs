#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Queue
{
	public sealed class IncomingMessageEnvelope
	{
		public readonly ICollection<MessagePart> MessageParts;
		public readonly CloudQueueMessage Original;
		public readonly bool ContainsBody;
		readonly byte[] _includedBody;
		readonly string _contract;
		readonly string _reference;
		public readonly string _topic;

		public byte[] IncludedBody
		{
			get
			{
				Enforce.That(ContainsBody, "Don't access body, when it is absent");
				return _includedBody;
			}
		}

		public string Contract
		{
			get
			{
				Enforce.That(_contract != null);
				return _contract;
			}
		}

		public string Reference
		{
			get
			{
				Enforce.NotNull(() => _reference);
				return _reference;
			}
		}
		public string Topic
		{
			get
			{
				Enforce.NotNull(() => _topic);
				return _topic;
			}
		}

		public IncomingMessageEnvelope(MessageBody message, CloudQueueMessage cloudMessage)
		{
			MessageParts = message.Parts;
			Original = cloudMessage;

			foreach (var part in MessageParts)
			{

				switch (part.Type)
				{
					case MessageType.ContractName:
						_contract = part.StringValue;
						break;
					case MessageType.BinaryBody:
						ContainsBody = true;
						_includedBody = part.BinaryValue;
						break;
					case MessageType.Topic:
						_topic = part.StringValue;
						break;
					case MessageType.StorageReference:
						_reference = part.StringValue;
						break;
				}
			}
		}

		
	}
}