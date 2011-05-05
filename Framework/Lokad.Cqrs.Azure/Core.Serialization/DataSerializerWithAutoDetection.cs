#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;

namespace Lokad.Cqrs.Core.Serialization
{
    /// <summary>
    /// Default message serializer that attempts to automatically detect serialization format.
    /// </summary>
    public sealed class DataSerializerWithAutoDetection : IDataSerializer
    {
        readonly IDataSerializer _serializer;

        public DataSerializerWithAutoDetection(IEnumerable<IKnowSerializationTypes> providers)
        {
            var types = providers.SelectMany(p => p.GetKnownTypes()).ToArray();

            var protoCount = types.Count(t => t.IsDefined(typeof (ProtoContractAttribute), false));
            var dataCount = types.Count(t => t.IsDefined(typeof (DataContractAttribute), false));

            if ((protoCount == 0) && (dataCount == 0))
            {
                throw new InvalidOperationException(
                    "Could not automatically detect serialization format. Please specify it manually");
            }

            if (protoCount > 0)
            {
                // protobuf takes precedence
                _serializer = new DataSerializerWithProtoBuf(types);
            }
            else
            {
                _serializer = new DataSerializerWithDataContracts(types);
            }
        }

        void IDataSerializer.Serialize(object instance, Stream destinationStream)
        {
            _serializer.Serialize(instance, destinationStream);
        }

        object IDataSerializer.Deserialize(Stream sourceStream, Type type)
        {
            return _serializer.Deserialize(sourceStream, type);
        }

        bool IDataSerializer.TryGetContractNameByType(Type messageType, out string contractName)
        {
            return _serializer.TryGetContractNameByType(messageType, out contractName);
        }

        bool IDataSerializer.TryGetContractTypeByName(string contractName, out Type contractType)
        {
            return _serializer.TryGetContractTypeByName(contractName, out contractType);
        }
    }
}