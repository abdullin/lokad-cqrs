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

        readonly ConcurrentDictionary<string, ITapeStream> _writers =
            new ConcurrentDictionary<string, ITapeStream>();

        public BlobTapeStorageFactory(IAzureStorageConfig config, string containerName)
        {
            if (containerName.Any(Char.IsUpper))
                throw new ArgumentException("All letters in a container name must be lowercase.");

            _cloudBlobClient = config.CreateBlobClient();
            
            _containerName = containerName;
        }

        public ITapeStream GetOrCreateStream(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrWhiteSpace("name"))
                throw new ArgumentException("Incorrect value.", "name");

            return _writers.GetOrAdd(
                name,
                s =>
                    {
                        var container = _cloudBlobClient.GetContainerReference(_containerName);
                        return new BlobTapeStream(container, name);
                    });
        }

        public void InitializeForWriting()
        {
            var container = _cloudBlobClient.GetContainerReference(_containerName);
            container.CreateIfNotExist();
        }
    }
}
