using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class BlobTapeStorageFactory : ITapeStorageFactory
    {
        readonly CloudBlobClient _cloudBlobClient;
        readonly string _containerName;

        readonly ConcurrentDictionary<string, ISingleThreadTapeWriter> _writers =
            new ConcurrentDictionary<string, ISingleThreadTapeWriter>();

        public BlobTapeStorageFactory(IAzureStorageConfig config, string containerName)
        {
            if (containerName.Any(Char.IsUpper))
                throw new ArgumentException("All letters in a container name must be lowercase.");

            _cloudBlobClient = config.CreateBlobClient();
            
            _containerName = containerName;
        }

        public ITapeReader GetReader(string name)
        {
            var container = _cloudBlobClient.GetContainerReference(_containerName);

            return new BlobTapeReader(container, name);
        }

        public void Initialize()
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
