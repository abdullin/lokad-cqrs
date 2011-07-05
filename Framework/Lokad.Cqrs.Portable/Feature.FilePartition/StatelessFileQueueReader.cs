using System;
using System.IO;
using System.Linq;
using Lokad.Cqrs.Core.Inbox;
using Lokad.Cqrs.Core.Inbox.Events;

namespace Lokad.Cqrs.Feature.FilePartition
{
    public sealed class StatelessFileQueueReader
    {
        readonly IEnvelopeStreamer _streamer;
        readonly ISystemObserver _observer;

        //readonly CloudBlobContainer _cloudBlob;
        readonly Lazy<DirectoryInfo> _posionQueue;
        readonly DirectoryInfo _queue;
        readonly string _queueName;

        public string Name
        {
            get { return _queueName; }
        }

        public StatelessFileQueueReader(IEnvelopeStreamer streamer, ISystemObserver observer, Lazy<DirectoryInfo> posionQueue, DirectoryInfo queue, string queueName)
        {
            _streamer = streamer;
            _observer = observer;
            _posionQueue = posionQueue;
            _queue = queue;
            _queueName = queueName;
        }

        public GetEnvelopeResult TryGetMessage()
        {
            FileInfo message;
            try
            {
                message = _queue.EnumerateFiles().FirstOrDefault();
            }
            catch (Exception ex)
            {
                _observer.Notify(new FailedToReadMessage(ex, _queueName));
                return GetEnvelopeResult.Error();
            }

            if (null == message)
            {
                return GetEnvelopeResult.Empty;
            }

            try
            {
                var unpacked = DownloadPackage(message);
                return GetEnvelopeResult.Success(unpacked);
            }
            catch (IOException ex)
            {
                _observer.Notify(new FailedToAccessStorage(ex, _queue.Name, message.Name));
                return GetEnvelopeResult.Retry;
            }
            catch (Exception ex)
            {
                _observer.Notify(new EnvelopeDeserializationFailed(ex, _queue.Name, message.Name));
                // new poison details
                var poisonFile = Path.Combine(_posionQueue.Value.FullName, message.Name);
                message.MoveTo(poisonFile);
                return GetEnvelopeResult.Retry;
            }
        }

        EnvelopeTransportContext DownloadPackage(FileInfo message)
        {
            using (var stream = message.OpenRead())
            using (var mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                var envelope = _streamer.ReadAsEnvelopeData(mem.ToArray());
                return new EnvelopeTransportContext(message, envelope, _queueName);
            }
        }

        public void Initialize()
        {
            _queue.Create();
        }

        /// <summary>
        /// ACKs the message by deleting it from the queue.
        /// </summary>
        /// <param name="envelope">The message context to ACK.</param>
        public void AckMessage(EnvelopeTransportContext envelope)
        {
            if (envelope == null) throw new ArgumentNullException("message");
            ((FileInfo)envelope.TransportMessage).Delete();
        }
    }
}