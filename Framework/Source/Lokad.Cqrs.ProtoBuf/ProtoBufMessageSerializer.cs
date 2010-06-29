#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs.Domain;
using Lokad.Cqrs.Serialization;
using Lokad.Quality;

namespace Lokad.Cqrs.ProtoBuf
{
	public sealed class ProtoBufMessageSerializer : IMessageSerializer
	{
		readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
		readonly ICollection<Type> _knownTypes;
		readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();
		readonly IDictionary<Type, IFormatter> _type2Formatter = new Dictionary<Type, IFormatter>();

		public ProtoBufMessageSerializer(ICollection<Type> knownTypes)
		{
			_knownTypes = knownTypes;

			foreach (var type in _knownTypes)
			{
				var reference = ProtoBufUtil.GetContractReference(type);
				var formatter = ProtoBufUtil.CreateFormatter(type);

				_contract2Type.Add(reference, type);
				_type2Contract.Add(type, reference);
				_type2Formatter.Add(type, formatter);
			}
		}

		[UsedImplicitly]
		public ProtoBufMessageSerializer(IMessageDirectory directory) : this(GetDirectoryTypes(directory))
		{
		}

		static Type[] GetDirectoryTypes(IMessageDirectory directory)
		{
			return directory.Messages
				.Where(m => false == m.MessageType.IsAbstract)
				.ToArray(m => m.MessageType);
		}

		public void Serialize(object instance, Stream destination)
		{
			_type2Formatter
				.GetValue(instance.GetType())
				.ExposeException("Unknown object type {0}", instance.GetType())
				.Serialize(destination, instance);
		}

		public object Deserialize(Stream source, Type type)
		{
			return _type2Formatter
				.GetValue(type)
				.ExposeException("Unknown object type {0}", type)
				.Deserialize(source);
		}

		public string GetContractNameByType(Type messageType)
		{
			return _type2Contract[messageType];
		}

		public Type GetTypeByContractName(string contractName)
		{
			return _contract2Type[contractName];
		}
	}
}