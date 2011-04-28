#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class MessageEnvelopeBuilder
    {
        public readonly string EnvelopeId;
        public readonly IDictionary<string, object> Attributes = new Dictionary<string, object>();
        public DateTimeOffset DeliverOn;

        public readonly IList<MessageItemToSave> Items = new List<MessageItemToSave>();

        public MessageEnvelopeBuilder(string envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public void AddItem<T>(T item)
        {
            // add KVPs after
            var t = typeof (T);
            if (t == typeof (object))
            {
                t = item.GetType();
            }

            var messageItemToSave = new MessageItemToSave(t, item);
            Items.Add(messageItemToSave);
        }

        public void DelayBy(TimeSpan span)
        {
            DeliverOn = DateTimeOffset.UtcNow + span;
        }

        public static MessageEnvelopeBuilder FromItems(string envelopeId, params object[] items)
        {
            var builder = new MessageEnvelopeBuilder(envelopeId);
            foreach (var item in items)
            {
                builder.AddItem(item);
            }
            var created = DateTimeOffset.UtcNow;
            builder.Attributes.Add(EnvelopeAttributes.CreatedUtc, created);
            return builder;
        }

        public MessageEnvelope Build()
        {
            var attributes = new Dictionary<string, object>(Attributes);
            var items = new MessageItem[Items.Count];

            for (int i = 0; i < items.Length; i++)
            {
                var save = Items[i];
                var attribs = new Dictionary<string, object>(save.Attributes);
                items[i] = new MessageItem(save.MappedType, save.Content, attribs);
            }

            return new MessageEnvelope(EnvelopeId, attributes, items, DeliverOn);
        }
    }
}