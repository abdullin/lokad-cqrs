#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

namespace Lokad.Cqrs
{
	public interface IEnvelopeStreamer
	{
		byte[] SaveReferenceMessage(EnvelopeReference reference);
		byte[] SaveDataMessage(MessageEnvelope builder);
		bool TryReadAsReference(byte[] buffer, out EnvelopeReference reference);
		MessageEnvelope ReadDataMessage(byte[] buffer);
	}
}