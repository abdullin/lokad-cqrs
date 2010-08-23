#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;
using Lokad.Quality;

namespace Lokad
{
	/// <summary>
	/// Helper utilities for working with byte buffers
	/// </summary>
	public static class BufferUtil
	{
		/// <summary>
		/// Calculates simple hash code
		/// </summary>
		/// <param name="bytes">The bytes to hash.</param>
		/// <returns>hash for the buffer</returns>
		/// <exception cref="ArgumentNullException">if bytes are null</exception>
		public static int CalculateSimpleHashCode([NotNull] byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException("bytes");
			if (bytes.Length < 4)
			{
				return ShortHash(bytes);
			}

			return LongHash(bytes);
		}

		static int LongHash(byte[] bytes)
		{
			int result = bytes.Length.GetHashCode();
			unchecked
			{
				for (int i = 0; i < bytes.Length; i += 4)
				{
					var remains = bytes.Length - i;

					int sliceHash = remains >= 4
						? BitConverter.ToInt32(bytes, i)
						: BitConverter.ToInt32(bytes, bytes.Length - 4);
					result = (result*0x18d) ^ sliceHash;
				}
			}
			return result;
		}

		static int ShortHash(byte[] bytes)
		{
			var result = bytes.Length.GetHashCode();
			unchecked
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					result = (result*0x18d) ^ bytes[i].GetHashCode();
				}
			}
			return result;
		}
	}
}