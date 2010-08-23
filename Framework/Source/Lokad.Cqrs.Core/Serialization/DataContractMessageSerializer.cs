#region Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.

// Copyright (c) 2009-2010 LOKAD SAS. All rights reserved.
// You must not remove this notice, or any other, from this software.
// This document is the property of LOKAD SAS and must not be disclosed.

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Lokad.Quality;
using System.Linq;

namespace Lokad.Serialization
{
	/// <summary>
	/// Message serializer for the <see cref="DataContractSerializer"/>
	/// </summary>
	[UsedImplicitly]
	public class DataContractMessageSerializer : IMessageSerializer
	{
		readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
		readonly ICollection<Type> _knownTypes;
		readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();


		/// <summary>
		/// Initializes a new instance of the <see cref="DataContractMessageSerializer"/> class.
		/// </summary>
		/// <param name="knownTypes">The known types.</param>
		public DataContractMessageSerializer(ICollection<Type> knownTypes)
		{
			if (knownTypes.Count == 0)
				throw new InvalidOperationException("DataContractMessageSerializer requires some known types to serialize. Have you forgot to supply them?");

			_knownTypes = knownTypes;

			DataContractUtil.ThrowOnMessagesWithoutDataContracts(_knownTypes);

			foreach (var type in _knownTypes)
			{
				var reference = DataContractUtil.GetContractReference(type);
				_contract2Type.Add(reference, type);
				_type2Contract.Add(type, reference);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataContractMessageSerializer"/> class.
		/// </summary>
		/// <param name="know">The know.</param>
		public DataContractMessageSerializer(IKnowSerializationTypes know) : this(know.GetKnownTypes().ToSet())
		{
		}

		/// <summary>
		/// Serializes the object to the specified stream
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="destination">The destination stream.</param>
		public void Serialize(object instance, Stream destination)
		{
			var serializer = new DataContractSerializer(instance.GetType(), _knownTypes);

			//using (var compressed = destination.Compress(true))
			using (var writer = XmlDictionaryWriter.CreateBinaryWriter(destination, null, null, false))
			{
				serializer.WriteObject(writer, instance);
			}
		}

		/// <summary>
		/// Deserializes the object from specified source stream.
		/// </summary>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <returns>deserialized object</returns>
		public object Deserialize(Stream sourceStream, Type type)
		{
			var serializer = new DataContractSerializer(type, _knownTypes);
			
			using (var reader = XmlDictionaryReader.CreateBinaryReader(sourceStream, XmlDictionaryReaderQuotas.Max))
			{
				return serializer.ReadObject(reader);
			}
		}

		/// <summary>
		/// Gets the contract name by the type
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <returns>contract name (if found)</returns>
		public Maybe<string> GetContractNameByType(Type messageType)
		{
			return _type2Contract.GetValue(messageType);
		}

		/// <summary>
		/// Gets the type by contract name.
		/// </summary>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns>type that could be used for contract deserialization (if found)</returns>
		public Maybe<Type> GetTypeByContractName(string contractName)
		{
			return _contract2Type.GetValue(contractName);
		}
	}
}