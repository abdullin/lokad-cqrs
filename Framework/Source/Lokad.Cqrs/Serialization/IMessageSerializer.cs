#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using Lokad.Serialization;

namespace Lokad.Cqrs.Serialization
{
	public interface IMessageSerializer : IDataSerializer, IDataContractMapper
	{
	
	}
}