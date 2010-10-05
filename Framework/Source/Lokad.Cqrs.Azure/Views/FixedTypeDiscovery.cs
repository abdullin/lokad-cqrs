#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using Lokad.Serialization;

namespace Lokad.Cqrs.Views
{
	public sealed class FixedTypeDiscovery : IKnowSerializationTypes
	{
		readonly Type[] _types;

		public FixedTypeDiscovery(Type[] types)
		{
			_types = types;
		}

		public IEnumerable<Type> GetKnownTypes()
		{
			return _types;
		}
	}
}