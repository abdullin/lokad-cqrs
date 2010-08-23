#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lokad.Quality;

namespace Lokad.Serialization
{
	/// <summary>
	/// Message serializer that uses <see cref="BinaryFormatter"/>. It is recommended to user ProtocolBuffers instead.
	/// </summary>
	[UsedImplicitly]
	public sealed class BinaryMessageSerializer : IMessageSerializer
	{
		readonly BinaryFormatter _formatter = new BinaryFormatter();

		/// <summary>
		/// Serializes the object to the specified stream
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="destinationStream">The destination stream.</param>
		public void Serialize(object instance, Stream destinationStream)
		{
			_formatter.Serialize(destinationStream, instance);
		}

		/// <summary>
		/// Deserializes the object from specified source stream.
		/// </summary>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <returns>deserialized object</returns>
		public object Deserialize(Stream sourceStream, Type type)
		{
			return _formatter.Deserialize(sourceStream);
		}

		/// <summary>
		/// Gets the contract name by the type
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <returns>contract name (if found)</returns>
		public Maybe<string> GetContractNameByType(Type messageType)
		{
			return messageType.AssemblyQualifiedName;
		}

		/// <summary>
		/// Gets the type by contract name.
		/// </summary>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns>type that could be used for contract deserialization (if found)</returns>
		public Maybe<Type> GetTypeByContractName(string contractName)
		{
			return Type.GetType(contractName);
		}
	}
}