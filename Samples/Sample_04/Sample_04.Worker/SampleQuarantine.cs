#region (c) 2010-2011 Lokad. New BSD License

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Diagnostics;
using System.Text;
using Lokad.Cqrs.Core.Dispatch;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Feature.StreamingStorage;
using Newtonsoft.Json;

namespace Sample_04.Worker
{
    public sealed class SampleQuarantine: IEnvelopeQuarantine
    {
        readonly MemoryQuarantine _quarantine = new MemoryQuarantine();

        public SampleQuarantine(IStreamingRoot streamingRoot)
        {
            streamingRoot.GetContainer("sample-04-errors").Create();
        }

        public bool TryToQuarantine(EnvelopeTransportContext context, Exception ex)
        {
            bool quarantined = _quarantine.TryToQuarantine(context, ex);

            var builder = new StringBuilder();
            
            RenderAdditionalContent(builder, context);

            builder.AppendLine("[Exception]");
            builder.AppendLine(DateTime.UtcNow.ToString());
            builder.AppendLine(ex.ToString());
            builder.AppendLine();

            Trace.WriteLine(builder.ToString());

            return quarantined;
        }

        public void TryRelease(EnvelopeTransportContext context)
        {
            _quarantine.TryRelease(context);
        }

        static void RenderAdditionalContent(StringBuilder builder, EnvelopeTransportContext context)
        {
            var s = Environment.NewLine;
            
            builder.AppendLine("Created:  " + context.Unpacked.CreatedOnUtc + "UTC");
            builder.AppendLine("Envelope: " + context.Unpacked.EnvelopeId);
            builder.AppendLine("Queue:    " + context.QueueName);

            foreach (var attribute in context.Unpacked.GetAllAttributes())
            {
                builder.AppendLine(attribute.Key + ": " + attribute.Value);
            }

            foreach (var message in context.Unpacked.Items)
            {
                builder.AppendFormat("{0}. {1}{2}", message.Index, message.MappedType, s);

                foreach (var attribute in message.GetAllAttributes())
                {
                    builder.AppendFormat("  {0}: {1}{2}", attribute.Key, attribute.Value, s);
                }
                builder.AppendLine(JsonConvert.SerializeObject(message.Content, Formatting.Indented));
                builder.AppendLine();
            }
        }
    }
}