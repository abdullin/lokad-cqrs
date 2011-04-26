#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs
{
    /// <summary>
    /// Joins data serializer and contract mapper
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

        /// <summary>
        /// Gets the contract name by the type
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>contract name (if found)</returns>
        bool TryGetContractNameByType(Type messageType, out string contractName);

        /// <summary>
        /// Gets the type by contract name.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <returns>type that could be used for contract deserialization (if found)</returns>
        bool TryGetContractTypeByName(string contractName, out Type contractType);
    }
}