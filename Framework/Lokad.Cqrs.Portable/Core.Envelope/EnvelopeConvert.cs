#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Lokad.Cqrs.Core.Envelope
{
    static class EnvelopeConvert
    {
        public static ImmutableAttribute[] EnvelopeAttributesFromContract(
            ICollection<EnvelopeAttributeContract> attributes)
        {
            var list = new ImmutableAttribute[attributes.Count];

            var idx = 0;
            foreach (var attribute in attributes)
            {
                switch (attribute.Type)
                {
                    case EnvelopeAttributeTypeContract.Sender:
                        list[idx] = new ImmutableAttribute(MessageAttributes.EnvelopeSender, attribute.Value);
                        break;
                    case EnvelopeAttributeTypeContract.CustomString:
                        list[idx] = new ImmutableAttribute(attribute.Name, attribute.Value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                idx += 1;
            }
            return list;
        }

        public static IDictionary<string, string> ItemAttributesFromContract(IEnumerable<ItemAttributeContract> attributes)
        {
            return attributes.ToDictionary(attribute => attribute.Name, attribute => attribute.Value);
        }

        public static ItemAttributeContract[] ItemAttributesToContract(
            ICollection<KeyValuePair<string, string>> attributes)
        {
            var contracts = new ItemAttributeContract[attributes.Count];
            var pos = 0;

            foreach (var attrib in attributes)
            {
                switch (attrib.Key)
                {
                    default:
                        contracts[pos] = ItemAttributeValueToContract(attrib.Key, attrib.Value);
                        break;

                }

                pos += 1;
            }

            return contracts;
        }

        static ItemAttributeContract ItemAttributeValueToContract(string name, string value)
        {
            return new ItemAttributeContract()
                {
                    Name = name,
                    Value = value
                };
        }

        public static EnvelopeAttributeContract[] EnvelopeAttributesToContract(
            ICollection<ImmutableAttribute> attributes)
        {
            var contracts = new EnvelopeAttributeContract[attributes.Count];
            int pos = 0;

            foreach (var attrib in attributes)
            {
                switch (attrib.Key)
                {
                    case MessageAttributes.EnvelopeSender:
                        contracts[pos] = new EnvelopeAttributeContract
                            {
                                Type = EnvelopeAttributeTypeContract.Sender,
                                Value = attrib.Value
                            };
                        break;
                    default:
                        contracts[pos] = new EnvelopeAttributeContract
                                {
                                    Type = EnvelopeAttributeTypeContract.CustomString,
                                    Name = attrib.Key,
                                    Value = attrib.Value
                                };
                        break;
                }
                pos += 1;
            }

            return contracts;
        }
    }
}