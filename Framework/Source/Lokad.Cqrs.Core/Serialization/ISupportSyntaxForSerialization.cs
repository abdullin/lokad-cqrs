#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

namespace Lokad.Serialization
{
	/// <summary>
	/// Syntax support for .Serialization configurations.
	/// </summary>
	public interface ISupportSyntaxForSerialization
	{
		/// <summary>
		/// Registers the specified data serializer as singleton implementing <see cref="IMessageSerializer"/>, <see cref="IDataSerializer"/> and <see cref="IDataContractMapper"/>. It can import <see cref="IKnowSerializationTypes"/>
		/// </summary>
		/// <typeparam name="TSerializer">The type of the serializer.</typeparam>
		void RegisterSerializer<TSerializer>()
			where TSerializer : IMessageSerializer;
	}
}