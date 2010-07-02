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
	[ProtoContract]
	[Serializable]
	public class MessageAttribute
	{
		[ProtoMember(1, IsRequired = true)] public readonly MessageAttributeType Type;
		[ProtoMember(2)] public readonly string CustomAttributeName;

		[ProtoMember(3)]
		public readonly byte[] BinaryValue;
		[ProtoMember(4)]
		public readonly string StringValue;
		[ProtoMember(5)]
		public readonly long NumberValue;

		[UsedImplicitly]
		MessageAttribute()
		{
		}

		public MessageAttribute(MessageAttributeType type, string stringValue)
		{
			Type = type;
			StringValue = stringValue;
		}

		public MessageAttribute(MessageAttributeType type, byte[] binaryValue)
		{
			Type = type;
			BinaryValue = binaryValue;
		}

		public MessageAttribute(MessageAttributeType type, long numberValue)
		{
			Type = type;
			NumberValue = numberValue;
		}

		public MessageAttribute(string customAttributeName, byte[] binaryValue)
		{
			CustomAttributeName = customAttributeName;
			BinaryValue = binaryValue;
		}

		public MessageAttribute(string customAttributeName, string stringValue)
		{
			CustomAttributeName = customAttributeName;
			StringValue = stringValue;
		}

		public MessageAttribute(string customAttributeName, long numberValue)
		{
			CustomAttributeName = customAttributeName;
			NumberValue = numberValue;
		}
	}

	[ProtoContract]
	public sealed class MessageAttributes
	{
		[ProtoMember(1, DataFormat = DataFormat.Default)]
		public readonly MessageAttribute[] Attributes;

		public MessageAttributes(MessageAttribute[] attributes)
		{
			Attributes = attributes;
		}

		[UsedImplicitly]
		MessageAttributes()
		{
			Attributes = new MessageAttribute[0];
		}

	}

	
	public sealed class MessageAttributeBuilder
	{
		[ProtoMember(1)]
		readonly List<MessageAttribute> _list = new List<MessageAttribute>();

		public void AddCustomString(string key, string value)
		{
			_list.Add(new MessageAttribute(key, value));
		}

		public void AddRange(MessageAttribute[] attributes)
		{
			_list.AddRange(attributes);
		}

		public void AddIdentity(string identity)
		{
			Add(MessageAttributeType.Identity, identity);
		}
		public void AddContract(string identity)
		{
			Add(MessageAttributeType.ContractName, identity);
		}
		public void AddTopic(string topic)
		{
			Add(MessageAttributeType.Topic, topic);
		}
		public void AddSender(string sender)
		{
			Add(MessageAttributeType.Sender, sender);
		}
		public void Add(MessageAttributeType type, string value)
		{
			_list.Add(new MessageAttribute(type, value));
		}
		public  void Add(MessageAttributeType type, long value)
		{
			_list.Add(new MessageAttribute(type, value));
		}

		public void AddBlobReference(Uri container, string reference)
		{
			Add(MessageAttributeType.StorageContainer, container.ToString());
			Add(MessageAttributeType.StorageReference, reference);
		}

		public void AddCreated(DateTime created)
		{
			Add(MessageAttributeType.CreatedUtc, created.ToBinary());
		}

		public MessageAttributes Build()
		{
			var messageAttributes = _list.ToArray();
			return new MessageAttributes(messageAttributes);
		}
	}

	public static class ExtendAttributes
	{
		
		public static Maybe<string> GetLastString(this MessageAttribute[] attributes, MessageAttributeType type)
		{
			for (int i = attributes.Length - 1; i >= 0; i--)
			{
				var item = attributes[i];
				if (item.Type == type)
				{
					var value = item.StringValue;
					if (value == null)
						throw Errors.InvalidOperation("Required string attribute can't be null");
					return value;
				}
			}
			return Maybe<string>.Empty;
		}
	}




	public enum MessageAttributeType : uint 
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