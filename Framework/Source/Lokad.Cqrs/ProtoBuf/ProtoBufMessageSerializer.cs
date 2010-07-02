#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Linq;
using Lokad.Cqrs.Domain;
using Lokad.Cqrs.Serialization;
using Lokad.Quality;
using Lokad.Serialization;

namespace Lokad.Cqrs.ProtoBuf
{
	public sealed class ProtoBufMessageSerializer : ProtoBufSerializer, IMessageSerializer
	{
		[UsedImplicitly]
		public ProtoBufMessageSerializer(IMessageDirectory directory) : base(GetDirectoryTypes(directory))
		{
		}

		static Type[] GetDirectoryTypes(IMessageDirectory directory)
		{
			return directory.Messages
				.Where(m => false == m.MessageType.IsAbstract)
				.ToArray(m => m.MessageType);
		}
	}
}