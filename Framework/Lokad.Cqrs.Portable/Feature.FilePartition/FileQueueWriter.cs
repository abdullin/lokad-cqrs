#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.IO;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.FilePartition
{
    public sealed class FileQueueWriter : IQueueWriter
    {
        readonly DirectoryInfo _folder;
        readonly IEnvelopeStreamer _streamer;

        public string Name { get; private set; }

        public FileQueueWriter(DirectoryInfo folder, string name, IEnvelopeStreamer streamer)
        {
            _folder = folder;
            _streamer = streamer;
            Name = name;
        }

        public void PutMessage(ImmutableEnvelope envelope)
        {
            var fileName = string.Format("{0:yyyy-MM-dd-HH-mm-ss-ffff}-{1}", envelope.CreatedOnUtc, envelope.EnvelopeId);
            var full = Path.Combine(_folder.FullName, fileName);
            var data = _streamer.SaveEnvelopeData(envelope);
            File.WriteAllBytes(full, data);
        }

        public void Init()
        {
            if (!_folder.Exists)
            {
                _folder.Create();
            }
        }
    }
}