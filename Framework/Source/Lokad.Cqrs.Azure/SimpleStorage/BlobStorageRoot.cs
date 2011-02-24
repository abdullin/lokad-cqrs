using Lokad.Cqrs.Logging;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.SimpleStorage
{
	/// <summary>
	/// Windows Azure implementation of storage 
	/// </summary>
	public sealed class BlobStorageRoot : IStorageRoot
	{
		readonly CloudBlobClient _client;
		readonly ILogProvider _provider;

		/// <summary>
		/// Initializes a new instance of the <see cref="BlobStorageRoot"/> class.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="provider">The provider.</param>
		public BlobStorageRoot(CloudBlobClient client, ILogProvider provider)
		{
			_client = client;
			_provider = provider;
		}

		public IStorageContainer GetContainer(string name)
		{
			return new BlobStorageContainer(_client.GetBlobDirectoryReference(name));
		}
	}
}