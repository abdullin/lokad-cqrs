using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.StorageClient;
using Lokad.Cqrs.Evil;

namespace Lokad.Cqrs.Feature.AtomicStorage.Azure
{
	sealed class AzureAtomicStorageInitialization : IEngineProcess
	{
		readonly CloudBlobClient _client;
		readonly IAzureAtomicStorageStrategy _strategy;

		public AzureAtomicStorageInitialization(CloudBlobClient client, IAzureAtomicStorageStrategy strategy)
		{
			_client = client;
			_strategy = strategy;
		}

		public void Dispose()
		{
		}

		public void Initialize()
		{
			// initialize views

			var types = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetExportedTypes())
				.Where(t => typeof(IAtomicEntity).IsAssignableFrom(t))
				.Where(t => !t.IsAbstract);

			var folders = types
				.Select(t => _strategy.GetFolderForEntity(t))
				.ToSet();

			folders.Add(_strategy.GetFolderForSingleton());
			folders
				.AsParallel()
				.ForAll(t => _client.GetContainerReference(t).CreateIfNotExist());
		}

		public Task Start(CancellationToken token)
		{
			// don't do anything
			return new Task(() => { });
		}
	}
}