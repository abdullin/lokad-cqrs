using System.IO;
using Lokad.Cqrs.Core.Outbox;

namespace Lokad.Cqrs.Feature.FilePartition
{
    public sealed class FileQueueWriterFactory : IQueueWriterFactory
    {
        readonly FileStorageConfig _account;
        readonly IEnvelopeStreamer _streamer;
        readonly string _endpoint;

        public FileQueueWriterFactory(FileStorageConfig account, IEnvelopeStreamer streamer)
        {
            _account = account;
            _streamer = streamer;
            _endpoint = _account.AccountName;
        }

        public string Endpoint
        {
            get { return _endpoint; }
        }

        public IQueueWriter GetWriteQueue(string queueName)
        {
            var full = Path.Combine(_account.Folder.FullName, queueName);
            if (!Directory.Exists(full))
            {
                Directory.CreateDirectory(full);
            }
            return
                new FileQueueWriter(new DirectoryInfo(full), queueName, _streamer);
        }
    }
}