using System;
using System.IO;

namespace Bus2.Serialization
{
	public interface IMessageSerializer
	{
		void Serialize(object instance, Stream destination);
		object Deserialize(Stream source, Type type);
	}
}