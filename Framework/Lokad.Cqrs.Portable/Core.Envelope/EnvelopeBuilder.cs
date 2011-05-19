#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class EnvelopeBuilder
    {
        public readonly string EnvelopeId;
        readonly IDictionary<string, string> _attributes = new Dictionary<string, string>();
        DateTime _deliverOnUtc;

        public readonly IList<MessageBuilder> Items = new List<MessageBuilder>();

        
        public void AddString(string key, string value)
        {
            _attributes.Add(key, value);
        }

        public void AddString(string tag)
        {
            _attributes.Add(tag,null);
        }

        public EnvelopeBuilder(string envelopeId)
        {
            EnvelopeId = envelopeId;
        }

        public MessageBuilder AddItem<T>(T item)
        {
            // add KVPs after
            var t = typeof (T);
            if (t == typeof (object))
            {
                t = item.GetType();
            }

            var messageItemToSave = new MessageBuilder(t, item);
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

        public static EnvelopeBuilder FromItems(string envelopeId, params object[] items)
        {
            var builder = new EnvelopeBuilder(envelopeId);
            foreach (var item in items)
            {
                builder.AddItem(item);
            }
            
            
            return builder;
        }

        public ImmutableEnvelope Build()
        {
            var attributes = _attributes.Select(p => new ImmutableAttribute(p.Key, p.Value)).ToArray();
            var items = new ImmutableMessage[Items.Count];

            for (int i = 0; i < items.Length; i++)
            {
                var save = Items[i];
                var attribs = save.Attributes.Select(p => new ImmutableAttribute(p.Key, p.Value)).ToArray();
                items[i] = new ImmutableMessage(save.MappedType, save.Content, attribs, i);
            }
            var created = DateTime.UtcNow;
            return new ImmutableEnvelope(EnvelopeId, attributes, items, _deliverOnUtc, created);
        }
    }
}