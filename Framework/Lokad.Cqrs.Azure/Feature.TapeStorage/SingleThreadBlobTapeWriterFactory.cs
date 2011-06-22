using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class SingleThreadBlobTapeWriterFactory : ISingleThreadTapeWriterFactory
    {
        readonly CloudBlobClient _cloudBlobClient;
        readonly string _containerName;
        readonly ConcurrentDictionary<string, ISingleThreadTapeWriter> _writers =
            new ConcurrentDictionary<string, ISingleThreadTapeWriter>();

        public SingleThreadBlobTapeWriterFactory(CloudBlobClient cloudBlobClient, string containerName)
        {
            if (containerName.Any(Char.IsUpper))
                throw new ArgumentException("All letters in a container name must be lowercase.");

            _cloudBlobClient = cloudBlobClient;
            _containerName = containerName;
        }

        public void Init()
        {
            var container = _cloudBlobClient.GetContainerReference(_containerName);
            container.CreateIfNotExist();
        }

        public ISingleThreadTapeWriter GetOrCreateWriter(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            var container = _cloudBlobClient.GetContainerReference(_containerName);

            var writer = _writers.GetOrAdd(
                name,
                new SingleThreadBlobTapeWriter(container, name));

            return writer;
        }
    }
}
