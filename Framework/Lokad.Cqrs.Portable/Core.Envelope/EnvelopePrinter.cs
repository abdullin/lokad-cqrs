using System;
using System.Globalization;
using System.IO;

namespace Lokad.Cqrs.Core.Envelope
{
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