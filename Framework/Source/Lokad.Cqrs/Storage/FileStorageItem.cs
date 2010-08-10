using System;
using System.Globalization;
using System.IO;

namespace Lokad.Cqrs.Storage
{
	public sealed class FileStorageItem : IStorageItem
	{
		readonly FileInfo _file;

		public FileStorageItem(FileInfo file)
		{
			_file = file;
		}

		bool Satisfy(StorageCondition condition)
		{
			return GetUnconditionalInfo()
				.Convert(s => new LocalStorageInfo(s.LastModifiedUtc, s.ETag))
				.Convert(s => condition.Satisfy(s), () => condition.Satisfy());
		}

	
		//bool ExistingFileMathes()

		public void Write(Action<Stream> writer, StorageCondition condition)
		{
			_file.Refresh();

			ThrowIfContainerNotFound();
			ThrowIfConditionFailed(condition);

			using (var file = _file.OpenWrite())
			{
				writer(file);
			}
		}

		public void ReadInto(ReaderDelegate reader, StorageCondition condition)
		{
			_file.Refresh();

			ThrowIfContainerNotFound();
			ThrowIfItemNotFound();
			ThrowIfConditionFailed(condition);

			var props = GetUnconditionalInfo().Value;
			using (var read = _file.OpenRead())
			{
				reader(props, read);
			}
		}

		void ThrowIfConditionFailed(StorageCondition condition)
		{
			if (!Satisfy(condition))
				throw StorageErrors.ConditionFailed(this, condition);
		}

		void ThrowIfItemNotFound()
		{
			if (!_file.Exists)
				throw StorageErrors.ItemNotFound(this);
		}

		public void Delete(StorageCondition condition)
		{
			_file.Refresh();

			ThrowIfContainerNotFound();

			if (_file.Exists && Satisfy(condition))
				_file.Delete();
		}

		public Maybe<StorageItemInfo> GetInfo(StorageCondition condition)
		{
			_file.Refresh();

			ThrowIfContainerNotFound();

			if (_file.Exists && Satisfy(condition))
				return GetUnconditionalInfo();
			return Maybe<StorageItemInfo>.Empty;
		}

		Maybe<StorageItemInfo> GetUnconditionalInfo()
		{
			if (!_file.Exists)
				return Maybe<StorageItemInfo>.Empty;

			var lastWriteTimeUtc = _file.LastWriteTimeUtc;
			var tag = lastWriteTimeUtc.ToString("R", CultureInfo.InvariantCulture);

			return new StorageItemInfo(lastWriteTimeUtc, tag);
		}

		public IStorageItem CopyFrom(IStorageItem sourceItem, StorageCondition condition, StorageCondition copySourceCondition)
		{
			var item = sourceItem as FileStorageItem;

			if (item != null)
			{
				_file.Refresh();
				ThrowIfContainerNotFound();
				ThrowIfConditionFailed(condition);

				item._file.Refresh();
				item.ThrowIfContainerNotFound();
				item.ThrowIfItemNotFound();
				item.ThrowIfConditionFailed(copySourceCondition);

				item._file.CopyTo(_file.FullName, true);
			}
			else
			{
				int bufferSize = 64.Kb();
				Write(targetStream => sourceItem.ReadInto((props, stream) => stream.PumpTo(targetStream, bufferSize), copySourceCondition), condition);

			}
			return this;
		}

		void ThrowIfContainerNotFound()
		{
			if (!_file.Directory.Exists)
				throw StorageErrors.ContainerNotFound(this);
		}

		public string FullPath
		{
			get { return _file.FullName; }
		}
	}
}