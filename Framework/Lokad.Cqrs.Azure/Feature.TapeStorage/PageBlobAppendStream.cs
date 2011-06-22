using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    using PageDataLengthType = Int16; // Same as in PageBlobReadStream

    public class PageBlobAppendStream : Stream
    {
        public const PageDataLengthType PageSize = 512;
        public const PageDataLengthType SizeOfPageDataSize = sizeof(PageDataLengthType);
        public const PageDataLengthType PageDataSize = PageSize - SizeOfPageDataSize;

        readonly CloudPageBlob _blob;

        long _lastPageIndex;
        readonly BinaryReader _reader;
        long _blobLength;
        Page _cachedPage;
        long _cachedPageIndex = -1;

        public PageBlobAppendStream(CloudPageBlob blob)
        {
            _blob = blob;
            if (blob == null)
                throw new ArgumentNullException("blob");

            _reader = new BinaryReader(_blob.OpenRead());
            if (!blob.Exists())
            {
                _blob.Create(0);
                _lastPageIndex = -1;
                return;
            }

            _blobLength = _blob.Properties.Length;
            _lastPageIndex = (_blobLength / PageSize) - 1;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long newLength)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var page = ReadPage(_lastPageIndex);

            var writePageIndex = _lastPageIndex;
            if (page == null || page.FreeSpace == 0)
            {
                page = new Page();
                writePageIndex++;
            }

            var rest = count;
            var bufferOffset = offset;

            var pages = new List<Page> {page};

            do
            {
                var bytesToWrite = (PageDataLengthType) Math.Min(rest, page.FreeSpace);
                rest -= bytesToWrite;

                page.Append(buffer, bufferOffset, bytesToWrite);
                bufferOffset += bytesToWrite;

                var needMorePages = rest > 0 && page.FreeSpace == 0;
                if (!needMorePages)
                    continue;

                page = new Page();
                pages.Add(page);
            } while (rest > 0);

            if (pages.Count * PageSize > 4 * 1024 * 1024)
                throw new NotSupportedException("Writing more than 4 Mb not supported.");

            var pagesAdded = pages.Count - 1 + writePageIndex - _lastPageIndex;
            if (pagesAdded > 0)
            {
                _blobLength += pagesAdded * PageSize;
                _blob.SetLength(_blobLength);
            }

            WritePages(pages, writePageIndex);
            _lastPageIndex += pagesAdded;
        }

        void WritePages(ICollection<Page> pages, long startPageIndex)
        {
            var buffer = new byte[pages.Count * PageSize];

            using (var ms = new MemoryStream(buffer))
            {
                foreach (var pageBuffer in pages.Select(page => page.GetBuffer()))
                    ms.Write(pageBuffer, 0, pageBuffer.Length);

                ms.Seek(0, SeekOrigin.Begin);
                var offset = startPageIndex * PageSize;
                _blob.WritePages(ms, offset);
            }

            _cachedPage = pages.Last();
            _cachedPageIndex = startPageIndex + pages.Count - 1;
        }

        Page ReadPage(long index)
        {
            if (index < 0)
                return null;

            if (index == _cachedPageIndex)
                return _cachedPage;

            var buffer = new byte[PageSize];

            _reader.BaseStream.Seek(index * PageSize, SeekOrigin.Begin);
            _reader.Read(buffer, 0, PageSize);

            return new Page(buffer);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get
            {
                var page = ReadPage(_lastPageIndex);
                if (page == null)
                    return 0;

                return _lastPageIndex * PageDataSize + page.Length;
            }
        }

        public override long Position
        {
            get { return Length; }
            set { throw new NotSupportedException(); }
        }

        class Page
        {
            readonly byte[] _data = new byte[PageDataSize];

            public Page()
            {
                Length = 0;
            }

            public Page(byte[] buffer)
            {
                using (var br = new BinaryReader(new MemoryStream(buffer)))
                {
                    br.BaseStream.Seek(0, SeekOrigin.Begin);
                    Length = br.ReadInt16(); // Must read PageDataLengthType type
                    br.Read(_data, 0, PageDataSize);
                }
            }

            public short Length { get; private set; }

            public PageDataLengthType FreeSpace
            {
                get { return (PageDataLengthType)(PageDataSize - Length); }
            }

            public void Append(byte[] buffer, int offset, PageDataLengthType count)
            {
                if (Length + count > PageDataSize)
                    throw new ArgumentOutOfRangeException("count");

                using (var bw = new BinaryWriter(new MemoryStream(_data)))
                {
                    bw.BaseStream.Seek(Length, SeekOrigin.Begin);
                    bw.Write(buffer, offset, count);
                    Length += count;
                }
            }

            public byte[] GetBuffer()
            {
                var bytes = new byte[PageSize];
                using (var bw = new BinaryWriter(new MemoryStream(bytes)))
                {
                    bw.Write(Length);
                    bw.Write(_data);

                    return bytes;
                }
            }
        }
    }
}
