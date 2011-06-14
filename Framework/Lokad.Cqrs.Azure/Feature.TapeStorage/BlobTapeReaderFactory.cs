using System;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    public class BlobTapeReaderFactory : ITapeReaderFactory
    {
        readonly CloudBlobClient _cloudBlobClient;
        readonly string _containerName;

        public BlobTapeReaderFactory(CloudBlobClient cloudBlobClient, string containerName)
        {
            if (containerName.Any(Char.IsUpper))
                throw new ArgumentException("All letters in a container name must be lowercase.");

            _cloudBlobClient = cloudBlobClient;
            _containerName = containerName;
        }

        public ITapeReader GetReader(string name)
        {
            var container = _cloudBlobClient.GetContainerReference(_containerName);

            return new BlobTapeReader(container, _containerName);
        }
    }
}
