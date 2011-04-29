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
        DateTimeOffset _deliverOn;

        public readonly IList<MessageItemBuilder> Items = new List<MessageItemBuilder>();

        public MessageEnvelopeBuilder(string envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public MessageItemBuilder AddItem<T>(T item)
        {
            // add KVPs after
            var t = typeof (T);
            if (t == typeof (object))
            {
                t = item.GetType();
            }

            var messageItemToSave = new MessageItemBuilder(t, item);
            Items.Add(messageItemToSave);
            return messageItemToSave;
        }

        public void DelayBy(TimeSpan span)
        {
            _deliverOn = DateTimeOffset.UtcNow + span;
        }

        public static MessageEnvelopeBuilder FromItems(string envelopeId, params object[] items)
        {
            var builder = new MessageEnvelopeBuilder(envelopeId);
            foreach (var item in items)
            {
                builder.AddItem(item);
            }
            
            
            return builder;
        }

        public ImmutableMessageEnvelope Build()
        {
            var attributes = new Dictionary<string, object>(Attributes);
            var items = new ImmutableMessageItem[Items.Count];

            for (int i = 0; i < items.Length; i++)
            {
                var save = Items[i];
                var attribs = new Dictionary<string, object>(save.Attributes);
                items[i] = new ImmutableMessageItem(save.MappedType, save.Content, attribs);
            }
            DateTimeOffset created = DateTimeOffset.UtcNow;
            return new ImmutableMessageEnvelope(EnvelopeId, attributes, items, _deliverOn, created);
        }
    }
}