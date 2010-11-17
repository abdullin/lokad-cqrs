#region (c) 2010 Lokad Open Source - New BSD License 

// Copyright (c) Lokad 2010, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;
using System.Runtime.Remoting;

namespace Lokad.Cqrs.Storage
{
	sealed class SuppressFlushForStream : Stream
	{
		readonly Stream _inner;

		public SuppressFlushForStream(Stream inner)
		{
			_inner = inner;
		}

		public override bool CanRead
		{
			get { return _inner.CanRead; }
		}

		public override bool CanSeek
		{
			get { return _inner.CanSeek; }
		}

		public bool CanTimeout
		{
			get { return _inner.CanTimeout; }
		}

		public override bool CanWrite
		{
			get { return _inner.CanWrite; }
		}

		public override long Length
		{
			get { return _inner.Length; }
		}

		public override long Position
		{
			get { return _inner.Position; }
			set { _inner.Position = value; }
		}

		public int ReadTimeout
		{
			get { return _inner.ReadTimeout; }
			set { _inner.ReadTimeout = value; }
		}

		public int WriteTimeout
		{
			get { return _inner.WriteTimeout; }
			set { _inner.WriteTimeout = value; }
		}

		public object GetLifetimeService()
		{
			return _inner.GetLifetimeService();
		}

		public object InitializeLifetimeService()
		{
			return _inner.InitializeLifetimeService();
		}

		public ObjRef CreateObjRef(Type requestedType)
		{
			return _inner.CreateObjRef(requestedType);
		}

		public void CopyTo(Stream destination)
		{
			_inner.CopyTo(destination);
		}

		public void CopyTo(Stream destination, int bufferSize)
		{
			_inner.CopyTo(destination, bufferSize);
		}

		public void Close()
		{
			_inner.Close();
		}

		public void Dispose()
		{
			_inner.Dispose();
		}

		public override void Flush()
		{
			// yeah, that's just the hack
			//_inner.Flush();
		}

		public IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _inner.BeginRead(buffer, offset, count, callback, state);
		}

		public int EndRead(IAsyncResult asyncResult)
		{
			return _inner.EndRead(asyncResult);
		}

		public IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			return _inner.BeginWrite(buffer, offset, count, callback, state);
		}

		public void EndWrite(IAsyncResult asyncResult)
		{
			_inner.EndWrite(asyncResult);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _inner.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_inner.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return _inner.Read(buffer, offset, count);
		}

		public int ReadByte()
		{
			return _inner.ReadByte();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_inner.Write(buffer, offset, count);
		}

		public void WriteByte(byte value)
		{
			_inner.WriteByte(value);
		}
	}
}