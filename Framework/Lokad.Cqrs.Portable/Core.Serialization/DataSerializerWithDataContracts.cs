#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace Lokad.Cqrs.Core.Serialization
{
    /// <summary>
    /// Message serializer for the <see cref="DataContractSerializer"/>
    /// </summary>
    public class DataSerializerWithDataContracts : IDataSerializer
    {
        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
        readonly ICollection<Type> _knownTypes;
        readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();


        /// <summary>
        /// Initializes a new instance of the <see cref="DataSerializerWithDataContracts"/> class.
        /// </summary>
        /// <param name="knownTypes">The known types.</param>
        public DataSerializerWithDataContracts(ICollection<Type> knownTypes)
        {
            if (knownTypes.Count == 0)
                throw new InvalidOperationException(
                    "DataContractMessageSerializer requires some known types to serialize. Have you forgot to supply them?");

            _knownTypes = knownTypes;

            ThrowOnMessagesWithoutDataContracts(_knownTypes);

            foreach (var type in _knownTypes)
            {
                var reference = ContractEvil.GetContractReference(type);
                try
                {
                    _contract2Type.Add(reference, type);
                }
                catch(ArgumentException ex)
                {
                    var format = String.Format("Failed to add contract reference '{0}'", reference);
                    throw new InvalidOperationException(format, ex);
                }
                
                _type2Contract.Add(type, reference);
            }
        }

        static void ThrowOnMessagesWithoutDataContracts(IEnumerable<Type> knownTypes)
        {
            var failures = knownTypes
                .Where(m => false == m.IsDefined(typeof(DataContractAttribute), false))
                .ToList();

            if (failures.Any())
            {
                var list = String.Join(Environment.NewLine, failures.Select(f => f.FullName).ToArray());

                throw new InvalidOperationException(
                    "All messages must be marked with the DataContract attribute in order to be used with DCS: " + list);
            }
        }

        /// <summary>
        /// Serializes the object to the specified stream
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="destination">The destination stream.</param>
        public void Serialize(object instance, Stream destination)
        {
            var serializer = new DataContractSerializer(instance.GetType(), _knownTypes);

            //using (var compressed = destination.Compress(true))
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(destination, null, null, false))
            {
                serializer.WriteObject(writer, instance);
            }
        }

        /// <summary>
        /// Deserializes the object from specified source stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="type">The type of the object to deserialize.</param>
        /// <returns>deserialized object</returns>
        public object Deserialize(Stream sourceStream, Type type)
        {
            var serializer = new DataContractSerializer(type, _knownTypes);

            using (var reader = XmlDictionaryReader.CreateBinaryReader(sourceStream, XmlDictionaryReaderQuotas.Max))
            {
                return serializer.ReadObject(reader);
            }
        }

        /// <summary>
        /// Gets the contract name by the type
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns>contract name (if found)</returns>
        public bool TryGetContractNameByType(Type messageType, out string contractName)
        {
            return _type2Contract.TryGetValue(messageType, out contractName);
        }

        /// <summary>
        /// Gets the type by contract name.
        /// </summary>
        /// <param name="contractName">Name of the contract.</param>
        /// <returns>type that could be used for contract deserialization (if found)</returns>
        public bool TryGetContractTypeByName(string contractName, out Type contractType)
        {
            return _contract2Type.TryGetValue(contractName, out contractType);
        }
    }
}