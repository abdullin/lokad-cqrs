#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using Lokad.Quality;
using ProtoBuf;

namespace Lokad.Cqrs.Queue
{
	[DataContract]
	[Serializable]
	public class MessagePart
	{
		[DataMember(Order = 1)] public readonly MessageType Type;
		[DataMember(Order = 2)] public readonly string StringKey;
		[DataMember(Order = 3)] public readonly byte[] BinaryValue;
		[DataMember(Order = 4)] public readonly string StringValue;

		[UsedImplicitly]
		MessagePart()
		{
		}

		public MessagePart(MessageType type, string stringKey, byte[] binaryValue, string stringValue)
		{
			Type = type;
			StringKey = stringKey;
			BinaryValue = binaryValue;
			StringValue = stringValue;
		}
	}

	public sealed class MessageParts
	{
		readonly List<MessagePart> _list = new List<MessagePart>();

		public void AddCustomString(string key, string value)
		{
			_list.Add(new MessagePart(MessageType.CustomString, key, null, value));
		}

		public void AddBody(byte[] body)
		{
			_list.Add(new MessagePart(MessageType.BinaryBody, null,body, null));
		}

		public void AddIdentity(string identity)
		{
			Add(MessageType.Identity, identity);
		}
		public void AddContract(string identity)
		{
			Add(MessageType.ContractName, identity);
		}
		public void AddTopic(string topic)
		{
			Add(MessageType.Topic, topic);
		}
		public void AddSender(string sender)
		{
			Add(MessageType.Sender, sender);
		}
		public void Add(MessageType type, string value)
		{
			_list.Add(new MessagePart(type, null, null, value));
		}

		public void AddBlobReference(Uri container, string reference)
		{
			_list.Add(new MessagePart(MessageType.StorageContainer, null, null, container.ToString()));
			_list.Add(new MessagePart(MessageType.StorageReference, null, null, reference));
		}

		public void AddCreated(DateTime created)
		{
			var binaryValue = created.ToString("r", CultureInfo.InvariantCulture);
			_list.Add(new MessagePart(MessageType.CreatedUtc, null, null, binaryValue));
		}

		public ICollection<MessagePart> AllParts
		{
			get { return _list; }
		}
	}




	public enum MessageType : byte 
	{
		Undefined = 0,
		ContractName = 1,
		ContractDefinition = 2,
		BinaryBody = 3,
		CustomString = 4,
		CustomBinary = 5,
		Topic = 6,
		Sender = 7,
		Identity = 8,
		CreatedUtc = 9,
		StorageReference = 10,
		StorageContainer = 11
	}
}