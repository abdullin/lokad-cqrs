using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Lokad.Cqrs
{
	public sealed class MessageAttributeBuilder
	{
		[ProtoMember(1)]
		readonly List<MessageAttributeContract> _list = new List<MessageAttributeContract>();

		public void AddIdentity(string identity)
		{
			Add(MessageAttributeTypeContract.Identity, identity);
		}
		public void AddContract(string identity)
		{
			Add(MessageAttributeTypeContract.ContractName, identity);
		}
		public void AddTopic(string topic)
		{
			Add(MessageAttributeTypeContract.Topic, topic);
		}
		public void AddSender(string sender)
		{
			Add(MessageAttributeTypeContract.Sender, sender);
		}
		public void Add(MessageAttributeTypeContract type, string value)
		{
			_list.Add(new MessageAttributeContract(type, value));
		}
		public  void Add(MessageAttributeTypeContract type, long value)
		{
			_list.Add(new MessageAttributeContract(type, value));
		}

		public void AddBlobReference(Uri container, string reference)
		{
			Add(MessageAttributeTypeContract.StorageContainer, container.ToString());
			Add(MessageAttributeTypeContract.StorageReference, reference);
		}

		public void AddCreated(DateTime created)
		{
			Add(MessageAttributeTypeContract.CreatedUtc, created.ToBinary());
		}

		public MessageAttributesContract Build()
		{
			var messageAttributes = _list.ToArray();
			return new MessageAttributesContract(messageAttributes);
		}
	}
}