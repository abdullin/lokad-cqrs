#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

#if !SILVERLIGHT2

using System;
using System.IO;
using System.IO.Compression;

namespace Lokad
{
	/// <summary>
	/// Simple helper extensions for the <see cref="Stream"/>
	/// </summary>
	public static class ExtendStream
	{
		/// <summary>
		/// Wraps the specified stream with Compression stream
		/// </summary>
		/// <param name="stream">The stream to compress</param>
		/// <returns>compressing stream</returns>
		public static GZipStream Compress(this Stream stream)
		{
			return new GZipStream(stream, CompressionMode.Compress);
		}

		/// <summary>
		/// Wraps the specified stream with Compression stream
		/// </summary>
		/// <param name="stream">The stream the stream to compress.</param>
		/// <param name="leaveOpen"><c>true</c> to leave the stream open; overwise <c>false</c>.</param>
		/// <returns>compressing stream</returns>
		public static GZipStream Compress(this Stream stream, bool leaveOpen)
		{
			return new GZipStream(stream, CompressionMode.Compress, leaveOpen);
		}

		/// <summary>
		/// Wraps the stream with Decompressing stream
		/// </summary>
		/// <param name="stream">The stream to decompress.</param>
		/// <returns>decompressing stream</returns>
		public static GZipStream Decompress(this Stream stream)
		{
			return new GZipStream(stream, CompressionMode.Decompress);
		}

		/// <summary>
		/// Wraps the stream with Decompressing stream
		/// </summary>
		/// <param name="stream">The stream to decompress.</param>
		/// <param name="leaveOpen"><c>true</c> to leave the stream open; overwise <c>false</c>.</param>
		/// <returns>decompressing stream</returns>
		public static GZipStream Decompress(this Stream stream, bool leaveOpen)
		{
			return new GZipStream(stream, CompressionMode.Decompress, leaveOpen);
		}

		/// <summary>
		/// Copies contents of this stream to the target stream
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="target">The target.</param>
		/// <param name="bufferSize">Size of the buffer.</param>
		/// <returns>total amount of bytes copied</returns>
		public static long PumpTo(this Stream source, Stream target, int bufferSize)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (target == null) throw new ArgumentNullException("target");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException("bufferSize", "Size of the buffer must be positive");

			return source.PumpTo(target, new byte[bufferSize]);
		}

		/// <summary>
		/// Copies contents of this stream to the target stream, using the provided buffer
		/// </summary>
		/// <param name="source">The source.</param>
		/// <param name="target">The target.</param>
		/// <param name="buffer">The buffer.</param>
		/// <returns>total amount of bytes copied</returns>
		public static long PumpTo(this Stream source, Stream target, byte[] buffer)
		{
			if (buffer == null) throw new ArgumentNullException("buffer");
			int bufferSize = buffer.Length;
			long total = 0;
			int count;
			while (0 < (count = source.Read(buffer, 0, bufferSize)))
			{
				target.Write(buffer, 0, count);
				total += count;
			}
			return total;
		}
	}
}

#endif