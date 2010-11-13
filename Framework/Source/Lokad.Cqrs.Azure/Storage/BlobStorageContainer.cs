#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Storage
{
	public sealed class BlobStorageRoot : IStorageRoot
	{
		readonly CloudBlobClient _client;
		readonly ILogProvider _provider;

		public BlobStorageRoot(CloudBlobClient client, ILogProvider provider)
		{
			_client = client;
			_provider = provider;
		}

		public IStorageContainer GetContainer(string name)
		{
			return new BlobStorageContainer(_client.GetBlobDirectoryReference(name), _provider);
		}
	}

	public sealed class BlobStorageContainer : IStorageContainer
	{
		readonly CloudBlobDirectory _directory;
		readonly ILog _log;
		readonly ILogProvider _provider;

		public BlobStorageContainer(CloudBlobDirectory directory, ILogProvider provider)
		{
			_directory = directory;
			_provider = provider;
			_log = provider.LogForName(this);
		}

		public IStorageContainer GetContainer([NotNull] string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			return new BlobStorageContainer(_directory.GetSubdirectory(name), _provider);
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