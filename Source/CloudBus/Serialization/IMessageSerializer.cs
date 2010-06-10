#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Serialization
{
	public interface IMessageSerializer
	{
		void Serialize(object instance, Stream destination);
		object Deserialize(Stream source, Type type);

		string GetContractNameByType(Type messageType);
		Type GetTypeByContractName(string contractName);
	}
}