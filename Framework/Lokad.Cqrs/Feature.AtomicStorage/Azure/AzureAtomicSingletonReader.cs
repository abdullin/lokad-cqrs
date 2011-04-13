using System;
using Lokad.Cqrs.Evil;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.AtomicStorage.Azure
{
	public sealed class AzureAtomicSingletonReader<TView> : IAtomicSingletonReader<TView>
		where TView : IAtomicSingleton
	{
		readonly CloudBlobClient _client;
		readonly IAzureAtomicStorageStrategy _strategy;

		public AzureAtomicSingletonReader(CloudBlobClient client, IAzureAtomicStorageStrategy strategy)
		{
			_client = client;
			_strategy = strategy;
		}

		CloudBlob GetBlob()
		{
			var name = _strategy.GetNameForSingleton(typeof(TView));

			return _client
				.GetContainerReference(_strategy.GetFolderForSingleton())
				.GetBlobReference(name);
		}


		public Maybe<TView> Get()
		{
			var blob = GetBlob();
			string text;
			try
			{
				// no retries and small timeout
				text = blob.DownloadText(new BlobRequestOptions
					{
						RetryPolicy = RetryPolicies.NoRetry(),
						Timeout = TimeSpan.FromSeconds(3)
					});
			}
			catch (StorageClientException ex)
			{
				return Maybe<TView>.Empty;
			}
			return _strategy.Deserialize<TView>(text);
		}
	}
}