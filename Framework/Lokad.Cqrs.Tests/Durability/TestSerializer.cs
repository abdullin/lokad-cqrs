#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Lokad.Cqrs.Core.Envelope;

namespace Lokad.Cqrs.Durability
{
    sealed class TestSerializer : IDataSerializer, IEnvelopeSerializer
    {
        static readonly BinaryFormatter Formatter = new BinaryFormatter();

        public void Serialize(object instance, Stream destinationStream)
        {
            Formatter.Serialize(destinationStream, instance);
        }

        public object Deserialize(Stream sourceStream, Type type)
        {
            return Formatter.Deserialize(sourceStream);
        }

        public bool TryGetContractNameByType(Type messageType, out string contractName)
        {
            contractName = messageType.FullName;
            return true;
        }

        public bool TryGetContractTypeByName(string contractName, out Type contractType)
        {
            contractType = Type.GetType(contractName);
            return true;
        }

        public static IDataSerializer Data = new TestSerializer();
        public static IEnvelopeSerializer Envelope = new TestSerializer();
        public static IEnvelopeStreamer Streamer = new EnvelopeStreamer(Envelope, Data);

        public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
        {
            Formatter.Serialize(stream, contract);
        }

        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            return (EnvelopeContract) Formatter.Deserialize(stream);
        }
    }
}