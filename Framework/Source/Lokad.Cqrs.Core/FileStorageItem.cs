#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs
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
			Refresh();

			ThrowIfContainerNotFound();
			ThrowIfConditionFailed(condition);

			using (var file = _file.OpenWrite())
			{
				writer(file);
			}
		}

		public void ReadInto(ReaderDelegate reader, StorageCondition condition)
		{
			Refresh();

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

		public void Remove(StorageCondition condition)
		{
			Refresh();

			ThrowIfContainerNotFound();

			if (_file.Exists && Satisfy(condition))
				_file.Delete();
		}

		public Maybe<StorageItemInfo> GetInfo(StorageCondition condition)
		{
			Refresh();
			ThrowIfContainerNotFound();

			if (_file.Exists && Satisfy(condition))
				return GetUnconditionalInfo();
			return Maybe<StorageItemInfo>.Empty;
		}

		Maybe<StorageItemInfo> GetUnconditionalInfo()
		{
			if (!_file.Exists)
				return Maybe<StorageItemInfo>.Empty;

			// yes, that's not full hashing, but for now we don't care
			var lastWriteTimeUtc = _file.LastWriteTimeUtc;
			var tag = string.Format("{0}-{1}", lastWriteTimeUtc.Ticks, _file.Length);

			return new StorageItemInfo(lastWriteTimeUtc, tag);
		}

		public void CopyFrom(IStorageItem sourceItem, StorageCondition condition, StorageCondition copySourceCondition)
		{
			var item = sourceItem as FileStorageItem;

			if (item != null)
			{
				Refresh();
				ThrowIfContainerNotFound();
				ThrowIfConditionFailed(condition);

				item.Refresh();
				item.ThrowIfContainerNotFound();
				item.ThrowIfItemNotFound();
				item.ThrowIfConditionFailed(copySourceCondition);

				item._file.CopyTo(_file.FullName, true);
			}
			else
			{
				var bufferSize = 64.Kb();
				Write(
					targetStream =>
						sourceItem.ReadInto((props, stream) => stream.PumpTo(targetStream, bufferSize), copySourceCondition), condition);
			}
		}

		void Refresh()
		{
			_file.Refresh();
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