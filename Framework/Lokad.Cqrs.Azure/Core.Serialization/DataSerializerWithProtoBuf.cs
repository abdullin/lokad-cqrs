#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Lokad.Cqrs.Evil;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Lokad.Cqrs.Core.Serialization
{
    public class DataSerializerWithProtoBuf : IDataSerializer
    {
        readonly IDictionary<string, Type> _contract2Type = new Dictionary<string, Type>();
        readonly IDictionary<Type, string> _type2Contract = new Dictionary<Type, string>();
        readonly IDictionary<Type, IFormatter> _type2Formatter = new Dictionary<Type, IFormatter>();

        public DataSerializerWithProtoBuf(ICollection<Type> knownTypes)
        {
            if (knownTypes.Count == 0)
                throw new InvalidOperationException(
                    "ProtoBuf requires some known types to serialize. Have you forgot to supply them?");
            foreach (var type in knownTypes)
            {
                var reference = ProtoBufUtil.GetContractReference(type);

                var formatter = RuntimeTypeModel.Default.CreateFormatter(type);
                

                _contract2Type.Add(reference, type);
                _type2Contract.Add(type, reference);
                _type2Formatter.Add(type, formatter);
            }
        }

        public static DataSerializerWithProtoBuf For<T>()
        {
            return new DataSerializerWithProtoBuf(new[] {typeof (T)});
        }

        public void Serialize(object instance, Stream destination)
        {
            IFormatter formatter;
            if (!_type2Formatter.TryGetValue(instance.GetType(), out formatter))
            {
                var s = string.Format("Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?", instance.GetType());
                throw new InvalidOperationException(s);
            }
            formatter.Serialize(destination, instance);
        }

        public object Deserialize(Stream source, Type type)
        {
            IFormatter value;
            if (!_type2Formatter.TryGetValue(type, out value))
            {
                var s = string.Format("Can't find serializer for unknown object type '{0}'. Have you passed all known types to the constructor?", type);
                throw new InvalidOperationException(s);
            }
            return value.Deserialize(source);
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