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
	/// Shortcuts for some common array operations
	/// </summary>
	public static class ExtendArray
	{
		/// <summary>
		/// Shorthand extension method for converting the arrays
		/// </summary>
		/// <typeparam name="TSource">The type of the source array.</typeparam>
		/// <typeparam name="TTarget">The type of the target array.</typeparam>
		/// <param name="source">The array to convert.</param>
		/// <param name="converter">The converter.</param>
		/// <returns>target array instance</returns>
		public static TTarget[] Convert<TSource, TTarget>(this TSource[] source, Converter<TSource, TTarget> converter)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (converter == null) throw new ArgumentNullException("converter");

			var outputArray = new TTarget[source.Length];
			for (int i = 0; i < source.Length; i++)
			{
				outputArray[i] = converter(source[i]);
			}
			return outputArray;
		}

		/// <summary>
		/// Shorthand extension method for converting the arrays
		/// </summary>
		/// <typeparam name="TSource">The type of the source array.</typeparam>
		/// <typeparam name="TTarget">The type of the target array.</typeparam>
		/// <param name="source">The array to convert.</param>
		/// <param name="converter">The converter, where the second parameter is an index of item being converted.</param>
		/// <returns>target array instance</returns>
		public static TTarget[] Convert<TSource, TTarget>(this TSource[] source, Func<TSource, int, TTarget> converter)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (converter == null) throw new ArgumentNullException("converter");

			var outputArray = new TTarget[source.Length];
			int i = 0;
			foreach (var input in source)
			{
				outputArray[i] = converter(input, i++);
			}
			return outputArray;
		}

		/// <summary>
		/// Applies the action to each item in the array
		/// </summary>
		/// <typeparam name="T">type of the items in the array</typeparam>
		/// <param name="self">The array to walk through.</param>
		/// <param name="action">The action.</param>
		/// <returns>Same array instance</returns>
		public static T[] ForEach<T>(this T[] self, Action<T> action)
		{
			if (self == null) throw new ArgumentNullException("self");
			if (action == null) throw new ArgumentNullException("action");

			foreach (var t in self)
			{
				action(t);
			}
			return self;
		}

		/// <summary>
		/// Slices array into array of arrays of length up to <paramref name="sliceLength"/>
		/// </summary>
		/// <typeparam name="T">Type of the items int the array</typeparam>
		/// <param name="array">The array.</param>
		/// <param name="sliceLength">Length of the slice.</param>
		/// <returns>array of sliced arrays</returns>
		/// <exception cref="ArgumentNullException">When source array is null</exception>
		/// <exception cref="ArgumentOutOfRangeException">When <paramref name="sliceLength"/> is invalid</exception>
		public static T[][] SliceArray<T>(this T[] array, int sliceLength)
		{
			if (array == null) throw new ArgumentNullException("array");

			if (sliceLength <= 0)
			{
				throw new ArgumentOutOfRangeException("sliceLength", "value must be greater than 0");
			}

			if (array.Length == 0)
				return new T[0][];

			int segments = array.Length/sliceLength;
			int last = array.Length%sliceLength;
			int totalSegments = segments + (last == 0 ? 0 : 1);

			var result = new T[totalSegments][];

			for (int i = 0; i < segments; i++)
			{
				var item = result[i] = new T[sliceLength];
				Array.Copy(array, i*sliceLength, item, 0, sliceLength);
			}
			if (last > 0)
			{
				var item = result[totalSegments - 1] = new T[last];
				Array.Copy(array, segments*sliceLength, item, 0, last);
			}

			return result;
		}

		/// <summary>
		/// Converts this array to a jagged array, while bringing indexing to zero-based.
		/// </summary>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="array">The array.</param>
		/// <returns>jagged array</returns>
		public static TValue[][] ToJaggedArray<TValue>([NotNull] this TValue[,] array)
		{
			if (array == null) throw new ArgumentNullException("array");
			var aMin = array.GetLowerBound(0);
			var aMax = array.GetUpperBound(0);
			var rows = aMax - aMin + 1;

			var bMin = array.GetLowerBound(1);
			var bMax = array.GetUpperBound(1);
			var cols = bMax - bMin + 1;

			var result = new TValue[rows][];

			for (int row = 0; row < rows; row++)
			{
				result[row] = new TValue[cols];
				for (int col = 0; col < cols; col++)
				{
					result[row][col] = array[row + aMin, col + bMin];
				}
			}
			return result;
		}
	}
}