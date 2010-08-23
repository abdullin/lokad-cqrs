#region (c)2009-2010 Lokad - New BSD license

// Copyright (c) Lokad 2009-2010 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using System.IO;

namespace Lokad.Serialization
{
	/// <summary>
	/// Generic data serializer interface.
	/// </summary>
	public interface IDataSerializer
	{
		/// <summary>
		/// Serializes the object to the specified stream
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="destinationStream">The destination stream.</param>
		void Serialize(object instance, Stream destinationStream);
		/// <summary>
		/// Deserializes the object from specified source stream.
		/// </summary>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="type">The type of the object to deserialize.</param>
		/// <returns>deserialized object</returns>
		object Deserialize(Stream sourceStream, Type type);
	}
}