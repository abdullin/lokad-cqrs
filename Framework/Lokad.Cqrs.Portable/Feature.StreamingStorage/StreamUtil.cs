#region (c) 2010-2011 Lokad - CQRS for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2011, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.IO;

namespace Lokad.Cqrs.Feature.StreamingStorage
{
    /// <summary>
    /// Simple helper extensions for the <see cref="Stream"/>
    /// </summary>
    public static class StreamUtil
    {
        /// <summary>
        /// Copies contents of this stream to the target stream
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        /// <returns>total amount of bytes copied</returns>
        public static long BlockCopy(Stream source, Stream target, int bufferSize)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (target == null) throw new ArgumentNullException("target");
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", "Size of the buffer must be positive");

            

            var buffer = new byte[bufferSize];
            if (buffer == null) throw new ArgumentNullException("buffer");
            var bufferSize1 = buffer.Length;
            long total = 0;
            int count;
            while (0 < (count = source.Read(buffer, 0, bufferSize1)))
            {
                target.Write(buffer, 0, count);
                total += count;
            }
            return total;
        }
    }
}