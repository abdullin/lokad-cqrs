using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Serialization
{
	/// <summary>
	/// Default message serializer that attempts to automatically detect serialization format.
	/// </summary>
	public sealed class AutoDetectingMessageSerializer : IMessageSerializer
	{
		readonly IMessageSerializer _serializer;

		public AutoDetectingMessageSerializer(IEnumerable<IKnowSerializationTypes> providers)
		{
			var types = providers.SelectMany(p => p.GetKnownTypes()).ToArray();

			var protoCount = types.Count(t => t.IsDefined(typeof (ProtoContractAttribute), false));
			var dataCount = types.Count(t => t.IsDefined(typeof (DataContractAttribute), false));

			if ((protoCount == 0) && (dataCount == 0))
			{
				throw new InvalidOperationException("Could not automatically detect serialization format. Please specify it manually");
			}

			if (protoCount > 0)
			{
				// protobuf takes precedence
				_serializer = new ProtoBufMessageSerializer(types);
			}
			else
			{
				_serializer = new DataContractMessageSerializer(types);
			}
		}

		void IMessageSerializer.Serialize(object instance, Stream destinationStream)
		{
			_serializer.Serialize(instance, destinationStream);
		}

		object IMessageSerializer.Deserialize(Stream sourceStream, Type type)
		{
			return _serializer.Deserialize(sourceStream, type);
		}

		Maybe<string> IMessageSerializer.GetContractNameByType(Type messageType)
		{
			return _serializer.GetContractNameByType(messageType);
		}

		Maybe<Type> IMessageSerializer.GetTypeByContractName(string contractName)
		{
			return _serializer.GetTypeByContractName(contractName);
		}
	}
}