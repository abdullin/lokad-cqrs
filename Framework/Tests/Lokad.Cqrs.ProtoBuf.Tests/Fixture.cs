using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.ProtoBuf.Tests
{

	public abstract class Fixture
	{
		protected T RoundTrip<T>(T item)
		{
			var formatter = FormatterCache<T>.Get();
			using (var memory = new MemoryStream())
			{
				formatter.Serialize(memory, item);
				memory.Seek(0, SeekOrigin.Begin);
				return (T) formatter.Deserialize(memory);
			}
		}


	}
}
