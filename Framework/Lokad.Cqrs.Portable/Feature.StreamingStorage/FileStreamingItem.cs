#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// File-based implementation of the <see cref="IStreamingItem"/>
    /// </summary>
    public sealed class FileStreamingItem : IStreamingItem
    {
        readonly FileInfo _file;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamingItem"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        public FileStreamingItem(FileInfo file)
        {
            _file = file;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileStreamingItem"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public FileStreamingItem(string filePath)
        {
            _file = new FileInfo(filePath);
        }

        bool Satisfy(StreamingCondition condition)
        {
            var info = GetUnconditionalInfo();
            return info
                .Convert(s => new LocalStreamingInfo(s.LastModifiedUtc, s.ETag))
                .Convert(s => condition.Satisfy(s), () => condition.Satisfy());
        }


        /// <summary>
        /// Performs the write operation, ensuring that the condition is met.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="options">The options.</param>
        /// <returns>number of bytes written</returns>
        /// <exception cref="StreamingItemIntegrityException">when integrity check fails during the upload</exception>
        public long Write(Action<Stream> writer, StreamingCondition condition, StreamingWriteOptions options)
        {
            Refresh();

            ThrowIfContainerNotFound();
            ThrowIfConditionFailed(condition);

            using (var file = _file.OpenWrite())
            {
                writer(file);
                // stream will probably be closed here.
            }
            Refresh();
            return _file.Length;
        }

        /// <summary>
        /// Attempts to read the storage item.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="condition">The condition.</param>
        /// <exception cref="StreamingItemNotFoundException">if the item does not exist.</exception>
        /// <exception cref="StreamingContainerNotFoundException">if the container for the item does not exist</exception>
        /// <exception cref="StreamingItemIntegrityException">when integrity check fails</exception>
        public void ReadInto(ReaderDelegate reader, StreamingCondition condition)
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

        void ThrowIfConditionFailed(StreamingCondition condition)
        {
            if (!Satisfy(condition))
                throw StreamingErrors.ConditionFailed(this, condition);
        }

        void ThrowIfItemNotFound()
        {
            if (!_file.Exists)
                throw StreamingErrors.ItemNotFound(this);
        }

        /// <summary>
        /// Removes the item, ensuring that the specified condition is met.
        /// </summary>
        /// <param name="condition">The condition.</param>
        public void Delete(StreamingCondition condition)
        {
            Refresh();

            ThrowIfContainerNotFound();

            if (_file.Exists && Satisfy(condition))
                _file.Delete();
        }

        /// <summary>
        /// Gets the info about this item. It returns empty result if the item does not exist or does not match the condition
        /// </summary>
        /// <param name="condition">The condition.</param>
        /// <returns></returns>
        public Optional<StreamingItemInfo> GetInfo(StreamingCondition condition)
        {
            Refresh();
            //ThrowIfContainerNotFound();

            if (_file.Exists && Satisfy(condition))
                return GetUnconditionalInfo();
            return Optional<StreamingItemInfo>.Empty;
        }

        Optional<StreamingItemInfo> GetUnconditionalInfo()
        {
            if (!_file.Exists)
                return Optional<StreamingItemInfo>.Empty;

            // yes, that's not full hashing, but for now we don't care
            var lastWriteTimeUtc = _file.LastWriteTimeUtc;
            var tag = string.Format("{0}-{1}", lastWriteTimeUtc.Ticks, _file.Length);

            return new StreamingItemInfo(lastWriteTimeUtc, tag, new NameValueCollection(0),
                new Dictionary<string, string>(0));
        }

        /// <summary>
        /// Creates this storage item from another.
        /// </summary>
        /// <param name="sourceItem">The target.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="copySourceCondition">The copy source condition.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="StreamingItemNotFoundException">when source storage is not found</exception>
        /// <exception cref="StreamingItemIntegrityException">when integrity check fails</exception>
        public void CopyFrom(IStreamingItem sourceItem, StreamingCondition condition, StreamingCondition copySourceCondition,
            StreamingWriteOptions options)
        {
            var item = sourceItem as FileStreamingItem;

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
                const int bufferSize = 64*1024;
                Write(
                    targetStream =>
                        sourceItem.ReadInto((props, stream) => StreamUtil.BlockCopy(stream, targetStream, bufferSize),
                            copySourceCondition), condition, options);
            }
        }

        void Refresh()
        {
            _file.Refresh();
        }

        void ThrowIfContainerNotFound()
        {
            if (!_file.Directory.Exists)
                throw StreamingErrors.ContainerNotFound(this);
        }

        /// <summary>
        /// Gets the full path of the current item.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath
        {
            get { return _file.FullName; }
        }

        /// <summary>
        /// Gets the file reference behind this instance.
        /// </summary>
        /// <value>The reference.</value>
        public FileInfo Reference
        {
            get { return _file; }
        }
    }
}