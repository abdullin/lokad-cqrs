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
		public readonly string MessageId;
		public readonly string Contract;
		public readonly Type MappedType;
		public readonly object Content;
		public readonly IDictionary<string, object> _attributes;

		public ICollection<KeyValuePair<string, object>> GetAllAttributes()
		{
			return _attributes;
		}

		public MessageItem(string messageId, string contract, Type mappedType, object content, IDictionary<string,object > attributes)
		{
			MessageId = messageId;
			Contract = contract;
			MappedType = mappedType;
			Content = content;
			_attributes = attributes;
		}
	}
}