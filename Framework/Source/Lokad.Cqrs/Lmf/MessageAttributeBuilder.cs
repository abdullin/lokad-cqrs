using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lokad.Cqrs
{
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
}