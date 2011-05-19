#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;

namespace Lokad.Cqrs.Core.Envelope
{
    [DataContract(Namespace = "Lokad.Cqrs.v2", Name = "Envelope"), Serializable]
    public sealed class EnvelopeContract
    {
        [DataMember(Order = 1)] public readonly string EnvelopeId;
        [DataMember(Order = 2)] public readonly EnvelopeAttributeContract[] EnvelopeAttributes;
        [DataMember(Order = 3)] public readonly MessageContract[] Messages;
        [DataMember(Order = 4)] public readonly DateTime DeliverOnUtc;
        [DataMember(Order = 5)] public readonly DateTime CreatedOnUtc;

        public EnvelopeContract(string envelopeId, EnvelopeAttributeContract[] envelopeAttributes, MessageContract[] messages,
            DateTime deliverOnUtc, DateTime createdOnUtc)
        {
            EnvelopeId = envelopeId;
            DeliverOnUtc = deliverOnUtc;
            EnvelopeAttributes = envelopeAttributes;
            Messages = messages;
            CreatedOnUtc = createdOnUtc;
        }

// ReSharper disable UnusedMember.Local
        EnvelopeContract()
// ReSharper restore UnusedMember.Local
        {
            Messages = NoMessages;
            EnvelopeAttributes = NoAttributes;
        }

        static readonly MessageContract[] NoMessages = new MessageContract[0];
        static readonly EnvelopeAttributeContract[] NoAttributes = new EnvelopeAttributeContract[0];
    }

    public static class EnvelopePrinter
    {
        public static string PrintToString(this ImmutableEnvelope envelope, Func<object,string> serializer)
        {
            using (var writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                PrintTo(envelope, writer, serializer);
                writer.Flush();
                return writer.GetStringBuilder().ToString();
            }
        }
        public static void PrintTo(this ImmutableEnvelope envelope, TextWriter writer, Func<object,string> serializer)
        {
            //.AppendFormat("{0,12}: {1}", attribute.GetName(), attribute.GetValue())
            writer.WriteLine(string.Format("{0,12}: {1}","EnvelopeId" ,envelope.EnvelopeId));
            writer.WriteLine(string.Format("{0,12}: {1:yyyy-MM-dd HH:mm:ss} (UTC)", "Created", envelope.CreatedOnUtc));
            if (envelope.DeliverOnUtc != DateTime.MinValue)
            {
                writer.WriteLine(string.Format("{0,12}: {1:yyyy-MM-dd HH:mm:ss} (UTC)", "Deliver On", envelope.DeliverOnUtc));
            }

            foreach (var attribute in envelope.GetAllAttributes())
            {
                writer.WriteLine(string.Format("{0,12}: {1}", attribute.Key, attribute.Value));
            }

            foreach (var message in envelope.Items)
            {
                writer.WriteLine();
                writer.WriteLine("{0}. {1}", message.Index, message.MappedType);

                foreach (var attribute in message.GetAllAttributes())
                {
                    writer.WriteLine(string.Format("{0,12}: {1}", attribute.Key, attribute.Value));
                }

                try
                {
                    var buffer = serializer(message.Content);
                    writer.WriteLine(buffer);
                }
                catch (Exception ex)
                {
                    writer.WriteLine("Rendering failure");
                    writer.WriteLine(ex);
                }

                writer.WriteLine();
            }
        }


    }
}