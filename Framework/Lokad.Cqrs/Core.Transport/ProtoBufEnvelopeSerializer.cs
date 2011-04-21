#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

namespace Lokad.Cqrs.Core.Transport
{
	public sealed class ProtoBufEnvelopeSerializer : IEnvelopeSerializer
	{
		readonly IMessageSerializer _serializer;

		public ProtoBufEnvelopeSerializer(IMessageSerializer serializer)
		{
			_serializer = serializer;
		}

		public byte[] SaveReferenceMessage(MessageReference reference)
		{
			return ProtoBufEnvelopeUtil.SaveReferenceMessage(reference);
		}

		public byte[] SaveDataMessage(MessageEnvelope builder)
		{
			return ProtoBufEnvelopeUtil.SaveDataMessage(builder, _serializer);
		}

		public bool TryReadAsReference(byte[] buffer, out MessageReference reference)
		{
			return ProtoBufEnvelopeUtil.TryReadAsReference(buffer, out reference);
		}

		public MessageEnvelope ReadDataMessage(byte[] buffer)
		{
			return ProtoBufEnvelopeUtil.ReadDataMessage(buffer, _serializer);
		}
	}
}