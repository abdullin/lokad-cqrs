using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Lokad.Cqrs.Core.Inbox;
using System.Linq;

namespace Lokad.Cqrs.Feature.FilePartition
{
    public sealed class FilePartitionInbox : IPartitionInbox
    {
        readonly DirectoryInfo[] _queues;
        readonly string[] _names;
        readonly IEnvelopeStreamer _serializer;

        public FilePartitionInbox(DirectoryInfo[] queues, string[] names, IEnvelopeStreamer serializer, Func<uint, TimeSpan> waiter)
        {
            _queues = queues;
            _names = names;
            _serializer = serializer;
            _waiter = waiter;
        }

        readonly Func<uint, TimeSpan> _waiter;
        uint _emptyCycles;

        public void Init()
        {
            foreach (var info in _queues)
            {
                if (!info.Exists)
                {
                    info.Create();
                }
            }
        }

        public void AckMessage(EnvelopeTransportContext envelope)
        {
            var file = (FileInfo) envelope.TransportMessage;
            file.Delete();
        }

        public bool TakeMessage(CancellationToken token, out EnvelopeTransportContext context)
        {
            while (!token.IsCancellationRequested)
            {
                // if incoming message is delayed and in future -> push it to the timer queue.
                // timer will be responsible for publishing back.

                

                try
                {

                    var fileInfo = _queues.SelectMany((q,i) => q.EnumerateFiles().Select(f => new{f,i})).FirstOrDefault();

                    if (null == fileInfo)
                    {
                        _emptyCycles += 1;
                    }
                    else
                    {
                        using (var stream= fileInfo.f.OpenRead())
                        using (var mem = new MemoryStream())
                        {
                            stream.CopyTo(mem);
                            var envelope = _serializer.ReadAsEnvelopeData(mem.ToArray());
                            if (envelope.DeliverOnUtc > DateTime.UtcNow)
                            {
                                // future message
                                throw new InvalidOperationException("Message scheduling has been disabled in the code");
                            }
                            context = new EnvelopeTransportContext(fileInfo.f, envelope, _names[fileInfo.i]);
                            return true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    
                }
                var waiting = _waiter(_emptyCycles);
                token.WaitHandle.WaitOne(waiting);

            }
            context = null;
            return false;
        }

        public void TryNotifyNack(EnvelopeTransportContext context)
        {
            
        }
    }
}