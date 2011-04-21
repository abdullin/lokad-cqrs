#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs.Core.Transport;
using Lokad.Cqrs.Envelope;
using Lokad.Cqrs.Evil;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Serialization
{
	public class ProtoBufDataSerializer : IDataSerializer
	{
		readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
		readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();
		readonly IDictionary<Type, IFormatter> _type2Formatter = new Dictionary<Type, IFormatter>();
		
		public ProtoBufDataSerializer(ICollection<Type> knownTypes)
		{
			if (knownTypes.Count == 0)
				throw new InvalidOperationException("ProtoBuf requires some known types to serialize. Have you forgot to supply them?");
			foreach (var type in knownTypes)
			{
				var reference = ProtoBufUtil.GetContractReference(type);
				var formatter = ProtoBufUtil.CreateFormatter(type);

				_contract2Type.Add(reference, type);
				_type2Contract.Add(type, reference);
				_type2Formatter.Add(type, formatter);
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ProtoBufDataSerializer"/> class.
		/// </summary>
		/// <param name="types">The types.</param>
		
		public ProtoBufDataSerializer(IEnumerable<IKnowSerializationTypes> types) : this (types.SelectMany(t => t.GetKnownTypes()).ToSet())
		{
		}

		public static ProtoBufDataSerializer For<T>()
		{
			return new ProtoBufDataSerializer(new[] { typeof(T)});
		}

		public void Serialize(object instance, Stream destination)
		{
			_type2Formatter
				.GetValue(instance.GetType())
				.ExposeException("Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?", instance.GetType())
				.Serialize(destination, instance);
		}

		public object Deserialize(Stream source, Type type)
		{
			return _type2Formatter
				.GetValue(type)
				.ExposeException("Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?", type)
				.Deserialize(source);
		}

		/// <summary>
		/// Gets the contract name by the type
		/// </summary>
		/// <param name="messageType">Type of the message.</param>
		/// <returns>contract name (if found)</returns>
		public bool TryGetContractNameByType(Type messageType, out string contractName)
		{
			return _type2Contract.TryGetValue(messageType, out contractName);
		}

		/// <summary>
		/// Gets the type by contract name.
		/// </summary>
		/// <param name="contractName">Name of the contract.</param>
		/// <returns>type that could be used for contract deserialization (if found)</returns>
		public bool TryGetContractTypeByName(string contractName, out Type contractType)
		{
			return _contract2Type.TryGetValue(contractName, out contractType);
		}
	}

	public sealed class ProtoBufEnvelopeSerializer : IEnvelopeSerializer
	{
		public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
		{
			Serializer.Serialize(stream, contract);
		}

		public EnvelopeContract DeserializeEnvelope(Stream stream)
		{
			return Serializer.Deserialize<EnvelopeContract>(stream);
		}
	}
}