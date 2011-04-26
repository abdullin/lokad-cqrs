#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Lokad.Cqrs.Core.Envelope
{
    public sealed class EnvelopeSerializerWithDataContracts : IEnvelopeSerializer
    {
        readonly DataContractSerializer _serializer;

        public EnvelopeSerializerWithDataContracts()
        {
            _serializer = new DataContractSerializer(typeof (EnvelopeContract));
        }

        public void SerializeEnvelope(Stream stream, EnvelopeContract contract)
        {
            //using (var compressed = destination.Compress(true))
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
            {
                _serializer.WriteObject(writer, contract);
            }
        }

        public EnvelopeContract DeserializeEnvelope(Stream stream)
        {
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
            {
                return (EnvelopeContract) _serializer.ReadObject(reader);
            }
        }
    }
}