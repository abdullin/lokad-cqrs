using System;
using System.IO;

namespace Lokad.Cqrs
{
	public interface IStorageItem
	{
		void Write(Action<Stream> writer, StorageCondition condition = default(StorageCondition));

		/// <summary>
		/// Attempts to read the storage item.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="condition">The condition.</param>
		/// <returns>false if the item exists, but the condition was not satisfied</returns>
		/// <exception cref="StorageItemNotFoundException">if the item does not exist.</exception>
		/// <exception cref="StorageContainerNotFoundException">if the container for the item does not exist</exception>
		void ReadInto(ReaderDelegate reader, StorageCondition condition = default(StorageCondition));

		void Remove(StorageCondition condition = default(StorageCondition));
		Maybe<StorageItemInfo> GetInfo(StorageCondition condition = default(StorageCondition));

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="sourceItem">The target.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="copySourceCondition">The copy source condition.</param>
		/// <returns></returns>
		/// <exception cref="StorageItemNotFoundException">when source storage is not found</exception>
		void CopyFrom(IStorageItem sourceItem, 
			StorageCondition condition = default(StorageCondition),
			StorageCondition copySourceCondition = default(StorageCondition));
		string FullPath { get; }
	}
}