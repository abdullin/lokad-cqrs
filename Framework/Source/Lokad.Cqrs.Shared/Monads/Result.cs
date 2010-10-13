#region (c)2009 Lokad - New BSD license

// Copyright (c) Lokad 2009 
// Company: http://www.lokad.com
// This code is released under the terms of the new BSD licence

#endregion

using System;

namespace Lokad
{
	/// <summary> Helper class for creating <see cref="Result{T}"/> instances </summary>
	[NoCodeCoverage]
	public static class Result
	{
		/// <summary> Creates success result </summary>
		/// <typeparam name="TValue">The type of the result.</typeparam>
		/// <param name="value">The item.</param>
		/// <returns>new result instance</returns>
		/// <seealso cref="Result{T}.CreateSuccess"/>
		[NotNull]
		public static Result<TValue> CreateSuccess<TValue>([NotNull] TValue value)
		{
			return Result<TValue>.CreateSuccess(value);
		}

		/// <summary> Creates success result </summary>
		/// <typeparam name="TValue">The type of the result.</typeparam>
		/// <param name="value">The item.</param>
		/// <returns>new result instance</returns>
		/// <seealso cref="Result{T}.CreateSuccess"/>
		[Obsolete("Use CreateSuccess instead")]
		public static Result<TValue> Success<TValue>([NotNull]TValue value)
		{
			return CreateSuccess(value);
		}
	}
}