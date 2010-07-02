using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Lokad.Quality;
using Lokad.Serialization;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.ProtoBuf.Tests
{

	public abstract class Fixture
	{
		[NotNull]
		static readonly ConcurrentDictionary<Type, IFormatter> Dict = new ConcurrentDictionary<Type, IFormatter>();

		static IFormatter GetFormatter(Type type)
		{
			return Dict.GetOrAdd(type, ProtoBufUtil.CreateFormatter);
		}

		protected T RoundTrip<T>(T item)
		{
			var formatter = GetFormatter(typeof(T));
			using (var memory = new MemoryStream())
			{
				formatter.Serialize(memory, item);
				memory.Seek(0, SeekOrigin.Begin);
				return (T) formatter.Deserialize(memory);
			}
		}

		protected T RoundTrip<T>(T item, Type legacy)
		{
			var formatter = GetFormatter(typeof(T));
			var via = GetFormatter(legacy);

			object intermediate;
			using (var memory = new MemoryStream())
			{
				formatter.Serialize(memory, item);
				memory.Seek(0, SeekOrigin.Begin);
				intermediate = via.Deserialize(memory);
			}

			using (var memory = new MemoryStream())
			{
				via.Serialize(memory, intermediate);
				memory.Seek(0, SeekOrigin.Begin);
				return (T)formatter.Deserialize(memory);
			}
		}


	}
}
