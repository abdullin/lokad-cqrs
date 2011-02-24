using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Durability
{
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