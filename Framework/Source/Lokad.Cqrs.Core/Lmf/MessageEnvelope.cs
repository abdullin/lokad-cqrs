#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Deserialized message representation
	/// </summary>
	public class MessageEnvelope
	{
		public readonly string EnvelopeId;

		readonly IDictionary<string, object> _attributes = new Dictionary<string, object>();

		public readonly MessageItem[] Items;

		public MessageEnvelope(string envelopeId, IDictionary<string,object> attributes, MessageItem[] items)
		{
			EnvelopeId = envelopeId;
			_attributes = attributes;
			Items = items;
		}

		public Maybe<TData> GetAttributeValue<TData>(string name)
			where TData : struct
		{
			return _attributes.GetValue(name).Convert(o => (TData)o);
		}

		public ICollection<KeyValuePair<string,object>> GetAllAttributes()
		{
			return _attributes;
		}
	}

	public sealed class MessageReference
	{
		public readonly string EnvelopeId;
		public readonly string StorageReference;
		public readonly string StorageContainer;

		public MessageReference(string envelopeId, string storageContainer, string storageReference)
		{
			EnvelopeId = envelopeId;
			StorageReference = storageReference;
			StorageContainer = storageContainer;
		}
	}

	public static class MessageAttributes
	{
		public static class Envelope
		{
			public const string CreatedUtc = "CreatedUtc";
			public const string Sender = "Sender";
			//public const string 
		}
		public static class Item
		{
			public const string Contract = "Contract";
		}

	}

	

	//public sealed class MessageEnvelope
	//{
	//    public readonly string EnvelopeId;
	//    public readonly MessageItem[] Messages;
	//    public readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();

	
	//}

	public sealed class MessageItem
	{
		public readonly string Contract;
		public readonly Type MappedType;
		public readonly object Content;
		readonly IDictionary<string, object> _attributes;

		public ICollection<KeyValuePair<string, object>> GetAllAttributes()
		{
			return _attributes;
		}

		public MessageItem(string contract, Type mappedType, object content, IDictionary<string,object > attributes)
		{
			Contract = contract;
			MappedType = mappedType;
			Content = content;
			_attributes = attributes;
		}
	}

	public sealed class MessageItemToSave
	{
		public readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();
		public readonly Type MappedType;
		public readonly object Content;

		public MessageItemToSave(Type mappedType, object content)
		{
			MappedType = mappedType;
			Content = content;
		}
	}

	public sealed class MessageEnvelopeBuilder
	{
		public readonly Guid EnvelopeId;
		public readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();

		public readonly IList<MessageItemToSave> Items = new List<MessageItemToSave>();

		public MessageEnvelopeBuilder(Guid envelopeId)
		{
			EnvelopeId = envelopeId;
		}

		public void AddItem<T>(T item)
		{
			var t = typeof (T);
			if (t == typeof(object))
			{
				t = item.GetType();
			}

			Items.Add(new MessageItemToSave(t, item));
		}


	}
}