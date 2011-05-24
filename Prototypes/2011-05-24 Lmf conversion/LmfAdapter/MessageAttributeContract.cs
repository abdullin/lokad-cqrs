#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using ProtoBuf;

namespace LmfAdapter
{
	[ProtoContract]
	[Serializable]
	public class MessageAttributeContract
	{
		[ProtoMember(1, IsRequired = true)] public readonly MessageAttributeTypeContract Type;
		[ProtoMember(2)] public readonly string CustomAttributeName;

		[ProtoMember(3)]
		public readonly byte[] BinaryValue;
		[ProtoMember(4)]
		public readonly string StringValue;
		[ProtoMember(5)]
		public readonly long NumberValue;

		MessageAttributeContract()
		{
		}

		public MessageAttributeContract(MessageAttributeTypeContract type, string stringValue)
		{
			Type = type;
			StringValue = stringValue;
		}

		public MessageAttributeContract(MessageAttributeTypeContract type, byte[] binaryValue)
		{
			Type = type;
			BinaryValue = binaryValue;
		}

		public MessageAttributeContract(MessageAttributeTypeContract type, long numberValue)
		{
			Type = type;
			NumberValue = numberValue;
		}

		public MessageAttributeContract(string customAttributeName, byte[] binaryValue)
		{
			CustomAttributeName = customAttributeName;
			BinaryValue = binaryValue;
		}

		public MessageAttributeContract(string customAttributeName, string stringValue)
		{
			CustomAttributeName = customAttributeName;
			StringValue = stringValue;
		}

		public MessageAttributeContract(string customAttributeName, long numberValue)
		{
			CustomAttributeName = customAttributeName;
			NumberValue = numberValue;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}", GetName(), GetValue());
		}

		public string GetName()
		{
			switch(Type)
			{
				case MessageAttributeTypeContract.CustomBinary:
				case MessageAttributeTypeContract.CustomNumber:
				case MessageAttributeTypeContract.CustomString:
					return CustomAttributeName;
				default:
					return Type.ToString();
			}
		}

		public object GetValue()
		{
			if (null != BinaryValue)
				return BinaryValue;
			if (null != StringValue)
				return StringValue;
			return NumberValue;
		}
	}
}