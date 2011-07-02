using System.IO;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.FilePartition
{
    public sealed class FileQueueWriterFactory : IQueueWriterFactory
    {
        readonly DirectoryInfo _account;
        readonly IEnvelopeStreamer _streamer;
        readonly string _endpoint;

        public FileQueueWriterFactory(DirectoryInfo account, IEnvelopeStreamer streamer, string endpoint = "file")
        {
            _account = account;
            _streamer = streamer;
            _endpoint = endpoint;
        }

        public string Endpoint
        {
            get { return _endpoint; }
        }

        public IQueueWriter GetWriteQueue(string queueName)
        {
            var full = Path.Combine(_account.FullName, queueName);
            return
                new FileQueueWriter(new DirectoryInfo(full),queueName, _streamer);
        }
    }
}