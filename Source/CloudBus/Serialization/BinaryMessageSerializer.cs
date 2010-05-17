#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lokad.Quality;

namespace CloudBus.Serialization
{
	[UsedImplicitly]
	public sealed class BinaryMessageSerializer : IMessageSerializer
	{
		readonly BinaryFormatter _formatter = new BinaryFormatter();

		public void Serialize(object instance, Stream destination)
		{
			_formatter.Serialize(destination, instance);
		}

		public object Deserialize(Stream source, Type type)
		{
			return _formatter.Deserialize(source);
		}

		public string GetContractNameByType(Type messageType)
		{
			return messageType.AssemblyQualifiedName;
		}

		public Type GetTypeByContractName(string contractName)
		{
			return Type.GetType(contractName);
		}
	}
}