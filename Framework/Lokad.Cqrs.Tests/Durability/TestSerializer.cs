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

		public Maybe<string> GetContractNameByType(Type messageType)
		{
			return messageType.FullName;
		}

		public Maybe<Type> GetTypeByContractName(string contractName)
		{
			return Type.GetType(contractName);
		}

		public static IMessageSerializer Instance = new TestSerializer();
	}
}