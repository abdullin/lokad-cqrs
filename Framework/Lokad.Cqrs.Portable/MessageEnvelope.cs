#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Transport
{
	/// <summary>
	/// Deserialized message representation
	/// </summary>
	public class MessageEnvelope
	{
		public readonly string EnvelopeId;
		public DateTimeOffset DeliverOn;
		readonly IDictionary<string, object> _attributes = new Dictionary<string, object>();
		public readonly MessageItem[] Items;

		public MessageEnvelope(string envelopeId, IDictionary<string,object> attributes, MessageItem[] items, DateTimeOffset deliverOn)
		{
			EnvelopeId = envelopeId;
			DeliverOn = deliverOn;
			_attributes = attributes;
			Items = items;
		}

		public object GetAttribute(string name)
		{
			return _attributes[name];
		}

		public ICollection<KeyValuePair<string,object>> GetAllAttributes()
		{
			return _attributes;
		}
	}
}