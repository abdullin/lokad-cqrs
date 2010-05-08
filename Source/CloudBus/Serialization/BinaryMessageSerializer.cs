using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lokad.Quality;

namespace Bus2.Serialization
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
	}
}