using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Durability
{
	public sealed class MessageEnvelopeBuilder
	{
		public readonly string EnvelopeId;
		public readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();

		public readonly IList<MessageItemToSave> Items = new List<MessageItemToSave>();

		public MessageEnvelopeBuilder(string envelopeId)
		{
			EnvelopeId = envelopeId;
		}

		public void AddItem<T>(T item)
		{
			// add KVPs after
			var t = typeof (T);
			if (t == typeof(object))
			{
				t = item.GetType();
			}

			var messageItemToSave = new MessageItemToSave(t, item);
			Items.Add(messageItemToSave);
		}
	}
}