#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Base interface for performing storage operations against local or remote persistence.
    /// </summary>
    public interface IStorageItem
    {
        /// <summary>
        /// Gets the full path of the current iteб.
        /// </summary>
        /// <value>The full path.</value>
        string FullPath { get; }

        /// <summary>
        /// Performs the write operation, ensuring that the condition is met.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="options">The options.</param>
        /// <returns>number of bytes written</returns>
        /// <exception cref="StorageItemIntegrityException">when integrity check fails during the upload</exception>
        long Write(Action<Stream> writer, StreamingCondition condition = default(StreamingCondition),
            StorageWriteOptions options = default(StorageWriteOptions));

        /// <summary>
        /// Attempts to read the storage item.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="condition">The condition.</param>
        /// <exception cref="StorageItemNotFoundException">if the item does not exist.</exception>
        /// <exception cref="StorageContainerNotFoundException">if the container for the item does not exist</exception>
        /// <exception cref="StorageItemIntegrityException">when integrity check fails</exception>
        void ReadInto(ReaderDelegate reader, StreamingCondition condition = default(StreamingCondition));

        /// <summary>
        /// Removes the item, ensuring that the specified condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        void Delete(StreamingCondition condition = default(StreamingCondition));

        /// <summary>
        /// Gets the info about this item. It returns empty result if the item does not exist or does not match the condition
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        Optional<StorageItemInfo> GetInfo(StreamingCondition condition = default(StreamingCondition));

        /// <summary>
        /// Creates this storage item from another.
        /// </summary>
        /// <param name="sourceItem">The target.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="copySourceCondition">The copy source condition.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="StorageItemNotFoundException">when source storage is not found</exception>
        /// <exception cref="StorageItemIntegrityException">when integrity check fails</exception>
        void CopyFrom(IStorageItem sourceItem,
            StreamingCondition condition = default(StreamingCondition),
            StreamingCondition copySourceCondition = default(StreamingCondition),
            StorageWriteOptions options = default(StorageWriteOptions));
    }
}