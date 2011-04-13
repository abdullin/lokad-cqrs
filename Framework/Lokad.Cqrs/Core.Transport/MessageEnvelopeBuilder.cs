using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Transport
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

		public static MessageEnvelopeBuilder FromItems(string envelopeId, params object[] items)
		{
			var builder = new MessageEnvelopeBuilder(envelopeId);
			foreach (var item in items)
			{
				builder.AddItem(item);
			}
			var created = DateTimeOffset.UtcNow;
			builder.Attributes.Add(MessageAttributes.Envelope.CreatedUtc, created);
			return builder;
		}

		public MessageEnvelope Build()
		{
			var attributes = new Dictionary<string, object>(Attributes);
			var items = new MessageItem[this.Items.Count];

			for (int i = 0; i < items.Length; i++)
			{
				var save = Items[0];
				var attribs = new Dictionary<string, object>(save.Attributes);
				items[i] = new MessageItem(save.MappedType, save.Content, attribs);
			}

			return new MessageEnvelope(EnvelopeId,attributes, items);
		}
	}
}