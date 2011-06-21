using System;
using System.ComponentModel;
using System.IO;
using Microsoft.WindowsAzure.StorageClient;

namespace Lokad.Cqrs.Feature.TapeStorage
{
    using PageDataLengthType = Int16; // Same as in PageBlobAppendStream

    public class PageBlobReadStream : Stream
    {
        readonly CloudPageBlob _blob;
        readonly BinaryReader _reader;
        long _pageIndex;
        PageDataLengthType _offset;

        public PageBlobReadStream(CloudPageBlob blob)
        {
            _blob = blob;

            if (!_blob.Exists())
                throw new ArgumentException();

            _reader = new BinaryReader(_blob.OpenRead());
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long streamOffset;
            long length = -1; // To cache Length's HTTP request
            switch (origin)
            {
                case SeekOrigin.Begin:
                    streamOffset = offset;
                    break;
                case SeekOrigin.Current:
                    streamOffset = Position + offset;
                    break;
                case SeekOrigin.End:
                    length = Length; // Length will send HTTP request
                    streamOffset = length - offset;
                    break;
                default:
                    throw new InvalidEnumArgumentException("origin", (int) origin, typeof(SeekOrigin));
            }

            if (length == -1)
                length = Length; // Length will send HTTP request

            if (streamOffset < 0 || streamOffset > length)
                throw new ArgumentOutOfRangeException("offset");

            Position = streamOffset;

            return streamOffset;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count == 0)
                return 0;

            if (Position + count > Length) // Length will send HTTP request
                return 0;

            int rem;
            var pagesToRead = Math.DivRem(_offset + count, PageBlobAppendStream.PageDataSize, out rem);
            if (rem > 0)
                pagesToRead++;

            var pagesBuffer = ReadPages(pagesToRead);

            using (var targetWriter = new BinaryWriter(new MemoryStream(buffer)))
            using (var pageReader = new BinaryReader(new MemoryStream(pagesBuffer)))
            {
                targetWriter.BaseStream.Seek(offset, SeekOrigin.Begin);

                var rest = count;

                do
                {
                    var pageDataSize = pageReader.ReadInt16(); // Must read PageDataLengthType type
                    // Move to current reading position
                    if (_offset != 0)
                        pageReader.ReadBytes(_offset);

                    var bytesToRead = (PageDataLengthType) (Math.Min(_offset + rest, pageDataSize) - _offset);
                    rest -= bytesToRead;

                    var bytes = pageReader.ReadBytes(bytesToRead);
                    targetWriter.Write(bytes);

                    _offset += bytesToRead;
                    if (_offset != PageBlobAppendStream.PageDataSize)
                        continue;

                    _offset = 0;
                    _pageIndex++;
                } while (rest > 0);
            }

            return count;
        }

        byte[] ReadPages(int count)
        {
            _reader.BaseStream.Seek(_pageIndex * PageBlobAppendStream.PageSize, SeekOrigin.Begin);
            var buffer = new byte[count * PageBlobAppendStream.PageSize];
            _reader.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get
            {
                _blob.FetchAttributes();
                var pageIndex = (_blob.Attributes.Properties.Length / PageBlobAppendStream.PageSize) - 1;

                _reader.BaseStream.Seek(pageIndex * PageBlobAppendStream.PageSize, SeekOrigin.Begin);
                var offset = _reader.ReadInt16(); // Must read PageDataLengthType type

                return pageIndex * PageBlobAppendStream.PageDataSize + offset;
            }
        }

        public override long Position
        {
            get
            {
                return _pageIndex * PageBlobAppendStream.PageDataSize + _offset;
            }
            set
            {
                long offset;
                _pageIndex = Math.DivRem(value, PageBlobAppendStream.PageDataSize, out offset);
                _offset = (PageDataLengthType) offset;
            }
        }
    }
}
