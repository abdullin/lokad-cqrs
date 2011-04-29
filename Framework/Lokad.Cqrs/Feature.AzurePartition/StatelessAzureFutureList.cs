#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using ProtoBuf;

namespace Lokad.Cqrs.Feature.AzurePartition
{
    public sealed class StatelessAzureFutureList
    {
        readonly ISystemObserver _observer;
        readonly IEnvelopeStreamer _streamer;

        readonly CloudBlobContainer _container;
        readonly CloudBlob _blob;


        public StatelessAzureFutureList(CloudBlobContainer container, ISystemObserver observer,
            IEnvelopeStreamer streamer)
        {
            _observer = observer;
            _streamer = streamer;
            _container = container;
            _blob = _container.GetBlobReference("__schedule.bin");
        }

        public void Initialize()
        {
            _container.CreateIfNotExist();
        }

        public void SetupForTesting()
        {
            foreach (var item in _container.ListBlobs())
            {
                ((CloudBlockBlob)item).Delete();
            }
        }

        public void PutMessage(ImmutableEnvelope envelope)
        {
            if (envelope.DeliverOnUtc == default(DateTime))
                throw new InvalidOperationException();


            ScheduleItem view;
            try
            {
                // use cached etags with stateful strategy, if needed

                var schedule = _blob.DownloadByteArray();
                using (var mem = new MemoryStream(schedule))
                {
                    view = Serializer.Deserialize<ScheduleItem>(mem);
                }
            }
            catch (StorageClientException ex)
            {
                view = new ScheduleItem();
            }
            var id = "__scheduled-" + envelope.EnvelopeId + ".bin";

            if (view.References.ContainsKey(id))
            {
                // already added
                return;
            }

            var bytes = _streamer.SaveDataMessage(envelope);
            _container.GetBlobReference(id).UploadByteArray(bytes);

            view.References.Add(id, envelope.DeliverOnUtc);
            using (var mem = new MemoryStream())
            {
                Serializer.Serialize(mem, view);
                _blob.UploadByteArray(mem.ToArray());
            }
        }

        public void TranferPendingMessages(Action<ImmutableEnvelope> atomicTranfer)
        {
            ScheduleItem view;
            try
            {
                // use cached etags with stateful strategy, if needed

                var schedule = _blob.DownloadByteArray();
                using (var mem = new MemoryStream(schedule))
                {
                    view = Serializer.Deserialize<ScheduleItem>(mem);
                }
            }
            catch (StorageClientException ex)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var pending = view.References.Where(r => r.Value <= now).ToArray();
            if (pending.Length == 0)
            {
                return;
            }

            // dispatch
            foreach (var pair in pending)
            {
                var item = _container.GetBlobReference(pair.Key).DownloadByteArray();
                var env = _streamer.ReadDataMessage(item);
                atomicTranfer(env);
                view.References.Remove(pair.Key);
            }
            // commit
            using (var mem = new MemoryStream())
            {
                Serializer.Serialize(mem, view);
                _blob.UploadByteArray(mem.ToArray());
            }
            // cleanup
            foreach (var pair in pending)
            {
                _container.GetBlobReference(pair.Key).BeginDeleteIfExists(ar => { }, null);
            }
        }

        [ProtoContract]
        public sealed class ScheduleItem
        {
            [ProtoMember(1)] public IDictionary<string, DateTimeOffset> References =
                new Dictionary<string, DateTimeOffset>();

            public ScheduleItem()
            {
                References = new Dictionary<string, DateTimeOffset>();
            }
        }
    }
}