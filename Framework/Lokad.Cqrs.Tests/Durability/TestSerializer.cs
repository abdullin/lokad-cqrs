using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Durability
{
	sealed class TestSerializer : IMessageSerializer
	{
		static readonly BinaryFormatter Formatter = new BinaryFormatter();
		public void Serialize(object instance, Stream destinationStream)
		{
			Formatter.Serialize(destinationStream, instance);
		}

		public object Deserialize(Stream sourceStream, Type type)
		{
			return Formatter.Deserialize(sourceStream);
		}

		public bool TryGetContractNameByType(Type messageType, out string contractName)
		{
			contractName = messageType.FullName;
			return true;
		}

		public bool TryGetContractTypeByName(string contractName, out Type contractType)
		{
			contractType = Type.GetType(contractName);
			return true;
		}

		public static IMessageSerializer Instance = new TestSerializer();
	}
}