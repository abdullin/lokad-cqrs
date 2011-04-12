#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs
{
	/// <summary>
	/// Joins data serializer and contract mapper
	/// </summary>
	public interface IMessageSerializer 
	{
		/// <summary>
		/// Serializes the object to the specified stream
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="destinationStream">The destination stream.</param>
		void Serialize(object instance, Stream destinationStream);
		/// <summary>
		/// Deserializes the object from specified source stream.
		/// </summary>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <returns>deserialized object</returns>
		object Deserialize(Stream sourceStream, Type type);

		/// <summary>
		/// Gets the contract name by the type
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <returns>contract name (if found)</returns>
		Maybe<string> GetContractNameByType(Type messageType);

		/// <summary>
		/// Gets the type by contract name.
		/// </summary>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns>type that could be used for contract deserialization (if found)</returns>
		Maybe<Type> GetTypeByContractName(string contractName);
	}
}