using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using Bus2.Domain;
using Lokad.Quality;

namespace Bus2.Serialization
{
	[UsedImplicitly]
	public sealed class DataContractMessageSerializer : IMessageSerializer
	{
		readonly ICollection<Type> _knownTypes;

		public DataContractMessageSerializer(ICollection<Type> knownTypes)
		{
			_knownTypes = knownTypes;

			DataContractUril.ThrowOnMessagesWithoutDataContracts(_knownTypes);
		}

		public DataContractMessageSerializer(IMessageDirectory directory)
		{
			_knownTypes = directory.Messages
				.Where(m => false == m.MessageType.IsAbstract)
				.ToArray(m => m.MessageType);

			DataContractUril.ThrowOnMessagesWithoutDataContracts(_knownTypes);

			
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
	}

	public sealed class JsonMessageSerializer : IMessageSerializer
	{
		public JsonMessageSerializer(IMessageDirectory directory)
		{
			var messages = directory.Messages
						.Where(m => false == m.MessageType.IsAbstract)
						.ToArray(m => m.MessageType);
		}

		public void Serialize(object instance, Stream destination)
		{
			throw new NotImplementedException();
		}

		public object Deserialize(Stream source, Type type)
		{
			throw new NotImplementedException();
		}
	}
}