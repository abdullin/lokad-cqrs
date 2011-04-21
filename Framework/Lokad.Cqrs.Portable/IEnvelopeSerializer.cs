#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

namespace Lokad.Cqrs
{
	public interface IEnvelopeSerializer
	{
		byte[] SaveReferenceMessage(MessageReference reference);
		byte[] SaveDataMessage(MessageEnvelope builder);
		bool TryReadAsReference(byte[] buffer, out MessageReference reference);
		MessageEnvelope ReadDataMessage(byte[] buffer);
	}
}