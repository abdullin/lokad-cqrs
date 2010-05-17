#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using CloudBus.Domain;
using Lokad.Quality;

namespace CloudBus.Serialization
{
	[UsedImplicitly]
	public sealed class DataContractMessageSerializer : IMessageSerializer
	{
		readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
		readonly ICollection<Type> _knownTypes;
		readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();


		public DataContractMessageSerializer(ICollection<Type> knownTypes)
		{
			_knownTypes = knownTypes;

			DataContractUtil.ThrowOnMessagesWithoutDataContracts(_knownTypes);
		}

		public DataContractMessageSerializer(IMessageDirectory directory)
		{
			_knownTypes = directory.Messages
				.Where(m => false == m.MessageType.IsAbstract)
				.ToArray(m => m.MessageType);

			DataContractUtil.ThrowOnMessagesWithoutDataContracts(_knownTypes);


			foreach (var type in _knownTypes)
			{
				var reference = DataContractUtil.GetContractReference(type);
				_contract2Type.Add(reference, type);
				_type2Contract.Add(type, reference);
			}
		}

		public void Serialize(object instance, Stream destination)
		{
			var serializer = new DataContractSerializer(instance.GetType(), _knownTypes);

			//using (var compressed = destination.Compress(true))
			using (var writer = XmlDictionaryWriter.CreateBinaryWriter(destination, null, null, false))
			{
				serializer.WriteObject(writer, instance);
			}
		}

		public object Deserialize(Stream source, Type type)
		{
			var serializer = new DataContractSerializer(type, _knownTypes);

			using (var reader = XmlDictionaryReader.CreateBinaryReader(source, XmlDictionaryReaderQuotas.Max))
			{
				return serializer.ReadObject(reader);
			}
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