#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class MessageEnvelopeBuilder
    {
        public readonly string EnvelopeId;
        internal readonly IDictionary<string, string> Attributes = new Dictionary<string, string>();
        DateTime _deliverOnUtc;

        public readonly IList<MessageItemBuilder> Items = new List<MessageItemBuilder>();

        
        public void AddString(string key, string value)
        {
            Attributes.Add(key, value);
        }

        public void AddString(string tag)
        {
            Attributes.Add(tag,null);
        }

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
            _deliverOnUtc = DateTime.UtcNow + span;
        }

        public void DeliverOnUtc(DateTime deliveryDateUtc)
        {
            _deliverOnUtc = deliveryDateUtc;
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

        public ImmutableEnvelope Build()
        {
            var attributes = Attributes.Select(p => new ImmutableAttribute(p.Key, p.Value)).ToArray();
            var items = new ImmutableMessage[Items.Count];

            for (int i = 0; i < items.Length; i++)
            {
                var save = Items[i];
                var attribs = new Dictionary<string, string>(save.Attributes);
                items[i] = new ImmutableMessage(save.MappedType, save.Content, attribs, i);
            }
            var created = DateTime.UtcNow;
            return new ImmutableEnvelope(EnvelopeId, attributes, items, _deliverOnUtc, created);
        }
    }
}