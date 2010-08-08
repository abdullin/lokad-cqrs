using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs
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

		public IStorageContainer GetContainer(string name)
		{
			Enforce.ArgumentNotEmpty(() => name);
			return new BlobStorageContainer(_directory.GetSubdirectory(name), _log);
		}

		public IStorageItem GetItem(string name)
		{
			Enforce.ArgumentNotEmpty(() => name);
			return new BlobStorageItem(_directory.GetBlobReference(name));
		}

		public IStorageContainer Create()
		{
			_directory.Container.CreateIfNotExist();
			return this;
		}

		public void Delete()
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