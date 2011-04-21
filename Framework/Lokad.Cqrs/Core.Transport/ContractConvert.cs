using System;
using System.Collections.Generic;
using Lokad.Cqrs.Envelope;

namespace Lokad.Cqrs.Core.Transport
{
	public static class ContractConvert
	{
		public static IDictionary<string, object> AttributesFromContract(IEnumerable<EnvelopeAttributeContract> attributes)
		{
			var dict = new Dictionary<string, object>();

			foreach (EnvelopeAttributeContract attribute in attributes)
			{
				switch (attribute.Type)
				{
					case EnvelopeAttributeTypeContract.CreatedUtc:
						dict[MessageAttributes.Envelope.CreatedUtc] = DateTimeOffset.Parse(attribute.StringValue);
						break;
					case EnvelopeAttributeTypeContract.Sender:
						dict[MessageAttributes.Envelope.Sender] = attribute.CustomName;
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

		public static IDictionary<string, object> AttributesFromContract(IEnumerable<ItemAttributeContract> attributes)
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

		public static ItemAttributeContract[] ItemAttributesToContract(ICollection<KeyValuePair<string, object>> attributes)
		{
			var contracts = new ItemAttributeContract[attributes.Count];
			int pos = 0;

			foreach (var attrib in attributes)
			{
				switch (attrib.Key)
				{
					default:
						contracts[pos] = new ItemAttributeContract();
						throw new NotImplementedException("serializing item attributes is not supported now");
				}

				pos += 1;
			}

			return contracts;
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
					case MessageAttributes.Envelope.CreatedUtc:
						contracts[pos] = new EnvelopeAttributeContract
							{
								Type = EnvelopeAttributeTypeContract.CreatedUtc,
								StringValue = ((DateTimeOffset)attrib.Value).ToString("o")
							};
						break;
					case MessageAttributes.Envelope.Sender:
						contracts[pos] = new EnvelopeAttributeContract
							{
								Type = EnvelopeAttributeTypeContract.Sender,
								StringValue = (string)attrib.Value
							};
						break;
					default:
						if (attrib.Value is string)
						{
							contracts[pos] = new EnvelopeAttributeContract
								{
									Type = EnvelopeAttributeTypeContract.CustomString,
									CustomName = attrib.Key,
									StringValue = (string)attrib.Value
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