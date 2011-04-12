using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.SimpleStorage.Blob
{
	/// <summary>
	/// Windows Azure implementation of storage 
	/// </summary>
	public sealed class BlobStorageRoot : IStorageRoot
	{
		readonly CloudBlobClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageRoot"/> class.
		/// </summary>
		/// <param name="client">The client.</param>
		public BlobStorageRoot(CloudBlobClient client)
		{
			_client = client;
		}

		public IStorageContainer GetContainer(string name)
		{
			return new BlobStorageContainer(_client.GetBlobDirectoryReference(name));
		}
	}
}