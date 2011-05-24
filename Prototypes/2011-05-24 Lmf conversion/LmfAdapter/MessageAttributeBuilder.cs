using System;
using System.Collections.Generic;
using ProtoBuf;

namespace LmfAdapter
{
	public sealed class MessageAttributeBuilder
	{
		[ProtoMember(1)]
		readonly List<MessageAttributeContract> _list = new List<MessageAttributeContract>();

		public MessageAttributeBuilder()
		{
		}

		public MessageAttributeBuilder(IEnumerable<MessageAttributeContract> attributes)
		{
			_list = new List<MessageAttributeContract>(attributes);
		}

		public MessageAttributeBuilder(MessageAttributesContract attributes) : this(attributes.Items)
		{

		}

		public void AddCustomString(string key, string value)
		{
			_list.Add(new MessageAttributeContract(key, value));
		}

		public void AddRange(IEnumerable<MessageAttributeContract> attributes)
		{
			_list.AddRange(attributes);
		}

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
		public void AddError(string error)
		{
			Add(MessageAttributeTypeContract.ErrorText, error);
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