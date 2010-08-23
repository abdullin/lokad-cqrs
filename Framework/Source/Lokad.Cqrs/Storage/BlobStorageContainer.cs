using System;
using Lokad.Quality;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Storage
{
	public sealed class BlobStorageContainer : IStorageContainer
	{
		readonly CloudBlobDirectory _directory;
		readonly ILog _log;

		public BlobStorageContainer(CloudBlobDirectory directory, ILog log)
		{
			_directory = directory;
			_log = log;
		}

		public IStorageContainer GetContainer([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new BlobStorageContainer(_directory.GetSubdirectory(name), _log);
		}

		public IStorageItem GetItem([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");
			return new BlobStorageItem(_directory.GetBlobReference(name));
		}

		public IStorageContainer Create()
		{
			_directory.Container.CreateIfNotExist();
			


			return this;
		}

		public void Remove()
		{
			try
			{
				_directory.Container.Delete();
			}
			catch (StorageClientException e)
			{
				switch (e.ErrorCode)
				{
					case StorageErrorCode.ContainerNotFound:
						return;
					default:
						throw;
				}
			}
		}

		public bool Exists()
		{
			try
			{
				_directory.Container.FetchAttributes();
				return true;
			}
			catch (StorageClientException e)
			{
				switch (e.ErrorCode)
				{
					case StorageErrorCode.ContainerNotFound:
					case StorageErrorCode.ResourceNotFound:
					case StorageErrorCode.BlobNotFound:
						return false;
					default:
						throw;
				}
			}
		}

		public string FullPath
		{
			get { return _directory.Uri.ToString(); }
		}
	}
}