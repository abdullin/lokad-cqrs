#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;

namespace Lokad.Cqrs.Core.Envelope
{
    static class EnvelopeConvert
    {
        public static IDictionary<string, object> EnvelopeAttributesFromContract(
            IEnumerable<EnvelopeAttributeContract> attributes)
        {
            var dict = new Dictionary<string, object>();

            foreach (var attribute in attributes)
            {
                switch (attribute.Type)
                {
                    case EnvelopeAttributeTypeContract.Sender:
                        dict[MessageAttributes.EnvelopeSender] = attribute.CustomName;
                        break;
                    case EnvelopeAttributeTypeContract.CustomString:
                        dict[attribute.CustomName] = attribute.StringValue;
                        break;
                    case EnvelopeAttributeTypeContract.CustomNumber:
                        dict[attribute.CustomName] = attribute.NumberValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return dict;
        }

        public static IDictionary<string, object> ItemAttributesFromContract(IEnumerable<ItemAttributeContract> attributes)
        {
            var dict = new Dictionary<string, object>();

            foreach (ItemAttributeContract attribute in attributes)
            {
                switch (attribute.Type)
                {
                    case ItemAttributeTypeContract.CustomString:
                        dict[attribute.CustomName] = attribute.StringValue;
                        break;
                    case ItemAttributeTypeContract.CustomNumber:
                        dict[attribute.CustomName] = attribute.NumberValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return dict;
        }

        public static ItemAttributeContract[] ItemAttributesToContract(
            ICollection<KeyValuePair<string, object>> attributes)
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

        static ItemAttributeContract ItemAttributeValueToContract(string name, object value)
        {
            if (value is string)
            {
                return new ItemAttributeContract
                {
                    Type = ItemAttributeTypeContract.CustomString,
                    CustomName = name,
                    StringValue = (string)value
                };
            }
            if ((value is long) || (value is int) || (value is short))
            {
                return new ItemAttributeContract
                    {
                        Type = ItemAttributeTypeContract.CustomNumber,
                        CustomName = name,
                        NumberValue = Convert.ToInt64(value)
                    };
            }
            throw new NotSupportedException("serialization of generic attributes is not supported yet");
        }

        public static EnvelopeAttributeContract[] EnvelopeAttributesToContract(
            ICollection<KeyValuePair<string, object>> attributes)
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
                                StringValue = (string) attrib.Value
                            };
                        break;
                    default:
                        if (attrib.Value is string)
                        {
                            contracts[pos] = new EnvelopeAttributeContract
                                {
                                    Type = EnvelopeAttributeTypeContract.CustomString,
                                    CustomName = attrib.Key,
                                    StringValue = (string) attrib.Value
                                };
                        }
                        else if ((attrib.Value is long) || (attrib.Value is int) || (attrib.Value is short))
                        {
                            contracts[pos] = new EnvelopeAttributeContract
                                {
                                    Type = EnvelopeAttributeTypeContract.CustomNumber,
                                    CustomName = attrib.Key,
                                    NumberValue = Convert.ToInt64(attrib.Value)
                                };
                        }
                        else
                        {
                            throw new NotSupportedException("serialization of generic attributes is not supported yet");
                        }
                        break;
                }
                pos += 1;
            }

            return contracts;
        }
    }
}