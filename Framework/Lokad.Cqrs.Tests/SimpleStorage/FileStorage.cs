using System;
using System.IO;
using Lokad.Cqrs.Feature.AzureStreamingStorage;
using Lokad.Cqrs.Feature.AzureStreamingStorage.Files;
using NUnit.Framework;

// ReSharper disable InconsistentNaming

namespace Lokad.Cqrs.Tests.Storage
{
	using Here = FileStorage;

	public sealed class FileStorage : ITestStorage
	{
		readonly DirectoryInfo _root;
		public FileStorage()
		{
			
			var root = Path.Combine(Path.GetTempPath(), "file-storage-tests");
			_root = new DirectoryInfo(root);
			_root.Create();

		}

		public IStorageContainer GetContainer(string name)
		{
			var combine = Path.Combine(_root.FullName, "test");
			return new FileStorageContainer(new DirectoryInfo(combine));
		}

		public StorageWriteOptions GetWriteHints()
		{
			return StorageWriteOptions.None;
		}

		[TestFixture]
		public sealed class When_deleting_blob_item :
			When_deleting_item_in<Here>
		{
		}

		[TestFixture]
		public sealed class When_reading_blob_item :
			When_reading_item_in<Here>
		{


		}

		[TestFixture]
		public sealed class When_writing_blob_item
			: When_writing_item_in<Here>
		{

		}

		[TestFixture]
		public sealed class When_copying_blob_item
			: When_copying_items_in<Here>
		{

		}
		[TestFixture]
		public sealed class When_checking_blob_item
			: When_checking_item_in<Here>
		{

		}
	}
}